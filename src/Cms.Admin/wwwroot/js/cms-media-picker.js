(function () {
  const state = {
    target: null,
    files: [],
    filter: "all"
  };

  /// A field that holds a list — several posters, say — must gain the new choice, not lose the
  /// ones already there. Marked with data-picker-append; every other field is replaced as before.
  function setTarget(url) {
    if (!state.target) return;
    if (state.target.dataset.pickerAppend === undefined || !state.target.value.trim()) {
      state.target.value = url;
      return;
    }
    state.target.value = state.target.value.trim().replace(/\s*\|\s*$/, "") + " | " + url;
  }

  function ensureModal() {
    let modal = document.getElementById("cms-media-picker");
    if (modal) return modal;

    modal = document.createElement("div");
    modal.id = "cms-media-picker";
    modal.className = "media-picker";
    modal.hidden = true;
    modal.innerHTML = `
      <div class="media-picker__dialog" role="dialog" aria-modal="true" aria-label="Media library">
        <header>
          <strong>Choose from media library</strong>
          <button type="button" class="btn btn-secondary btn-sm" data-picker-close>Close</button>
        </header>
        <div class="media-picker__filters">
          <button type="button" class="btn btn-secondary btn-sm" data-picker-filter="all">All</button>
          <button type="button" class="btn btn-secondary btn-sm" data-picker-filter="Image">Images</button>
          <button type="button" class="btn btn-secondary btn-sm" data-picker-filter="Document">Documents</button>
        </div>
        <label class="media-picker__upload">
          <input type="file" data-picker-upload accept="image/*,application/pdf" hidden />
          <span class="btn btn-sm">Upload a file</span>
          <small data-picker-status>Choose a picture from this computer, or pick one below.</small>
        </label>
        <div class="media-picker__grid" data-picker-grid></div>
        <p class="muted" data-picker-empty hidden>Nothing here yet — use "Upload a file" above to add your first picture.</p>
      </div>`;
    document.body.appendChild(modal);

    modal.addEventListener("click", (event) => {
      if (event.target === modal || event.target.closest("[data-picker-close]")) {
        closePicker();
      }
      const filter = event.target.closest("[data-picker-filter]");
      if (filter) {
        state.filter = filter.dataset.pickerFilter || "all";
        renderGrid();
      }
      const pick = event.target.closest("[data-picker-url]");
      if (pick && state.target) {
        setTarget(pick.dataset.pickerUrl || "");
        state.target.dispatchEvent(new Event("input", { bubbles: true }));
        state.target.dispatchEvent(new Event("change", { bubbles: true }));
        closePicker();
      }
    });
    // Uploading from inside the picker, because the library is empty on a new installation:
    // without this the button opens an empty box telling the operator to go and upload
    // somewhere else, which reads as "adding pictures does not work".
    modal.addEventListener("change", async (event) => {
      const chooser = event.target.closest("[data-picker-upload]");
      if (!chooser || !chooser.files || !chooser.files.length) return;

      const file = chooser.files[0];
      const status = modal.querySelector("[data-picker-status]");
      status.textContent = `Uploading ${file.name}…`;

      const body = new FormData();
      body.append("Upload", file);
      body.append("UploadKind", file.type === "application/pdf" ? "document" : "image");
      const token = document.querySelector("input[name=__RequestVerificationToken]");
      if (token) body.append("__RequestVerificationToken", token.value);

      try {
        const response = await fetch("/CMS/Media?handler=Upload", {
          method: "POST",
          body: body,
          headers: { "X-Requested-With": "XMLHttpRequest" }
        });
        if (!response.ok) throw new Error(`Upload failed (${response.status})`);

        await loadFiles(modal);
        // Select what was just uploaded, so one action finishes the job.
        const newest = state.files.find((item) => item.fileName === file.name);
        if (newest && state.target) {
          setTarget(newest.url);
          state.target.dispatchEvent(new Event("input", { bubbles: true }));
          state.target.dispatchEvent(new Event("change", { bubbles: true }));
          status.textContent = `${file.name} added.`;
          closePicker();
          return;
        }
        status.textContent = `${file.name} uploaded — choose it below.`;
      } catch (error) {
        status.textContent = error.message + ". The file may be too large or of an unsupported type.";
      } finally {
        chooser.value = "";
      }
    });

    return modal;
  }

  function closePicker() {
    const modal = document.getElementById("cms-media-picker");
    if (modal) modal.hidden = true;
    state.target = null;
  }

  function renderGrid() {
    const modal = ensureModal();
    const grid = modal.querySelector("[data-picker-grid]");
    const empty = modal.querySelector("[data-picker-empty]");
    const files = state.files.filter((file) =>
      state.filter === "all" || file.mediaType === state.filter);
    grid.innerHTML = files.map((file) => `
      <button type="button" class="media-picker__item" data-picker-url="${file.url}">
        ${file.mediaType === "Image"
          ? `<img src="${file.url}" alt="${file.fileName || ""}" />`
          : `<span class="media-picker__doc">PDF</span>`}
        <small>${file.fileName || file.url}</small>
      </button>`).join("");
    empty.hidden = files.length > 0;
  }

  async function loadFiles(modal) {
    const grid = modal.querySelector("[data-picker-grid]");
    try {
      const response = await fetch("/CMS/Media?handler=List", {
        headers: { Accept: "application/json", "X-Requested-With": "XMLHttpRequest" }
      });
      if (!response.ok) throw new Error("Failed to load media");
      state.files = await response.json();
      renderGrid();
    } catch (error) {
      grid.innerHTML = `<p class="muted">${error.message}</p>`;
    }
  }

  async function openPicker(input) {
    state.target = input;
    const modal = ensureModal();
    modal.hidden = false;
    modal.querySelector("[data-picker-grid]").innerHTML = "<p class='muted'>Loading media…</p>";
    await loadFiles(modal);
  }

  document.addEventListener("click", (event) => {
    const trigger = event.target.closest("[data-media-picker]");
    if (!trigger) return;
    event.preventDefault();
    const selector = trigger.getAttribute("data-media-picker");
    const input = selector ? document.querySelector(selector) : trigger.previousElementSibling;
    if (input) openPicker(input);
  });
})();

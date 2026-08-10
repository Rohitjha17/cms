(function () {
  const state = {
    target: null,
    files: [],
    filter: "all"
  };

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
        <div class="media-picker__grid" data-picker-grid></div>
        <p class="muted" data-picker-empty hidden>No media found. Upload files in Media library first.</p>
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
        state.target.value = pick.dataset.pickerUrl || "";
        state.target.dispatchEvent(new Event("input", { bubbles: true }));
        state.target.dispatchEvent(new Event("change", { bubbles: true }));
        closePicker();
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

  async function openPicker(input) {
    state.target = input;
    const modal = ensureModal();
    modal.hidden = false;
    const grid = modal.querySelector("[data-picker-grid]");
    grid.innerHTML = "<p class='muted'>Loading media…</p>";
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

  document.addEventListener("click", (event) => {
    const trigger = event.target.closest("[data-media-picker]");
    if (!trigger) return;
    event.preventDefault();
    const selector = trigger.getAttribute("data-media-picker");
    const input = selector ? document.querySelector(selector) : trigger.previousElementSibling;
    if (input) openPicker(input);
  });
})();

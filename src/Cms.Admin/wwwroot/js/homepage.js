(function () {
  const $ = (selector, root = document) => root.querySelector(selector);
  const $$ = (selector, root = document) => Array.from(root.querySelectorAll(selector));

  document.addEventListener("click", function (event) {
    const copyButton = event.target.closest("[data-copy-value]");
    if (copyButton) {
      navigator.clipboard?.writeText(copyButton.dataset.copyValue || "");
      const original = copyButton.textContent;
      copyButton.textContent = "Copied";
      window.setTimeout(() => { copyButton.textContent = original; }, 1400);
      return;
    }

    const dropdownTrigger = event.target.closest("[data-dropdown-trigger]");
    if (dropdownTrigger) {
      const dropdown = dropdownTrigger.closest("[data-dropdown]");
      const isOpen = dropdown.classList.toggle("is-open");
      dropdownTrigger.setAttribute("aria-expanded", String(isOpen));
      return;
    }

    $$("[data-dropdown].is-open").forEach((dropdown) => {
      if (!dropdown.contains(event.target)) {
        dropdown.classList.remove("is-open");
        $("[data-dropdown-trigger]", dropdown)?.setAttribute("aria-expanded", "false");
      }
    });

    if (event.target.closest("[data-sidebar-toggle]")) {
      $("#cms-sidebar")?.classList.toggle("is-open");
    }

    const deviceButton = event.target.closest("[data-device]");
    if (deviceButton) {
      const device = deviceButton.dataset.device;
      const stage = $("[data-preview-stage]");
      if (stage) stage.dataset.device = device;
      $$("[data-device]").forEach((button) => button.classList.toggle("is-active", button === deviceButton));
    }
  });

  function initSectionFilters() {
    const search = $("[data-section-search]");
    const status = $("[data-section-status]");
    const cards = $$("[data-section-card]");
    const empty = $("[data-section-empty]");
    const count = $("[data-section-count]");
    if (!search || cards.length === 0) return;

    const filter = () => {
      const query = search.value.trim().toLowerCase();
      const state = status?.value || "all";
      let visible = 0;
      cards.forEach((card) => {
        const matchesQuery = !query || card.dataset.search.includes(query);
        const matchesState = state === "all" || card.dataset.status === state;
        const show = matchesQuery && matchesState;
        card.hidden = !show;
        if (show) visible += 1;
      });
      if (count) count.textContent = `${visible} section${visible === 1 ? "" : "s"}`;
      if (empty) empty.hidden = visible !== 0;
    };

    search.addEventListener("input", filter);
    status?.addEventListener("change", filter);
  }

  function initSectionSorting() {
    const list = $("[data-sortable-sections]");
    const form = $("[data-reorder-form]");
    const toolbar = $("[data-reorder-toolbar]");
    const orderedKeys = $("[data-ordered-keys]");
    if (!list || !form || !toolbar || !orderedKeys) return;

    let dragged = null;
    const syncOrder = () => {
      orderedKeys.value = $$("[data-section-card]", list)
        .map((card) => card.dataset.sectionKey)
        .filter(Boolean)
        .join(",");
      toolbar.hidden = false;
    };

    list.addEventListener("dragstart", (event) => {
      const card = event.target.closest("[data-section-card]");
      if (!card) return;
      dragged = card;
      card.classList.add("is-dragging");
      event.dataTransfer.effectAllowed = "move";
      event.dataTransfer.setData("text/plain", card.dataset.sectionKey || "");
    });

    list.addEventListener("dragover", (event) => {
      if (!dragged) return;
      event.preventDefault();
      const target = event.target.closest("[data-section-card]");
      if (!target || target === dragged || target.hidden) return;
      const bounds = target.getBoundingClientRect();
      const insertAfter = event.clientY > bounds.top + bounds.height / 2;
      list.insertBefore(dragged, insertAfter ? target.nextSibling : target);
    });

    list.addEventListener("dragend", () => {
      if (!dragged) return;
      dragged.classList.remove("is-dragging");
      dragged = null;
      syncOrder();
    });
  }

  function initMenuBuilder() {
    const host = $("[data-menu-items]");
    const template = $("#menu-item-template");
    const add = $("[data-add-menu-item]");
    if (!host || !template || !add) return;

    const reindex = () => {
      $$("[data-menu-item]", host).forEach((item, index) => {
        $("strong", item).textContent = `Link ${index + 1}`;
        $$("input, select, textarea", item).forEach((input) => {
          const property = input.dataset.name
            || input.name?.match(/Input\.Items\[\d+\]\.([A-Za-z]+)/)?.[1];
          if (property) input.name = `Input.Items[${index}].${property}`;
        });
      });
    };

    add.addEventListener("click", () => {
      host.append(template.content.cloneNode(true));
      reindex();
    });
    host.addEventListener("click", (event) => {
      const remove = event.target.closest("[data-remove-menu-item]");
      if (!remove) return;
      remove.closest("[data-menu-item]")?.remove();
      reindex();
    });
    reindex();
  }

  function setMediaPreview(input, targetSelector) {
    input?.addEventListener("change", () => {
      const file = input.files?.[0];
      const target = $(targetSelector);
      if (!file || !target) return;
      const reader = new FileReader();
      reader.onload = () => {
        target.src = String(reader.result);
        target.closest(".media-preview")?.removeAttribute("hidden");
      };
      reader.readAsDataURL(file);
    });
  }

  document.addEventListener("DOMContentLoaded", () => {
    initSectionFilters();
    initSectionSorting();
    initMenuBuilder();
    setMediaPreview($("#image-file"), "#image-preview");
    setMediaPreview($("#bg-image-file"), "#background-preview");
    $$("[data-count]").forEach((input) => {
      const target = document.getElementById(input.dataset.count);
      const update = () => { if (target) target.textContent = String(input.value.length); };
      input.addEventListener("input", update);
      update();
    });
  });

  const sectionSchemas = {
    hero: [
      ["heading", "Hero heading", "text", "The main campaign headline"],
      ["description", "Supporting line", "text", "Short value proposition"],
      ["primaryButton", "Primary button", "text", "e.g. Apply now"],
      ["secondaryButton", "Secondary button", "text", "e.g. Explore campus"],
      ["autoplaySeconds", "Slide change (seconds)", "number", "Add images below. 0 stops the slideshow"],
      ["videoUrl", "Background video URL", "url", "YouTube, Vimeo or hosted video"]
    ],
    statistics: [
      ["students", "Students", "number", "Total enrolled students"],
      ["teachers", "Teachers", "number", "Faculty count"],
      ["placements", "Placements", "number", "Successful placements"],
      ["years", "Years of excellence", "number", "Years since establishment"]
    ],
    contact: [
      ["email", "Public email", "email", "General enquiry address"],
      ["phone", "Phone number", "tel", "Public contact number"],
      ["address", "Campus address", "text", "Full postal address"],
      ["mapEmbedUrl", "Map embed URL", "url", "Google Maps embed URL"]
    ],
    video: [
      ["videoUrl", "Video URL", "url", "YouTube, Vimeo or hosted video"],
      ["posterUrl", "Poster image URL", "url", "Preview image before playback"],
      ["caption", "Video caption", "text", "Accessible supporting caption"]
    ],
    admission_cta: [
      ["heading", "CTA heading", "text", "Admissions campaign headline"],
      ["supportingText", "Supporting text", "text", "One-line call to action"],
      ["deadline", "Application deadline", "date", "Current intake deadline"]
    ],
    download_brochure: [
      ["documentUrl", "Brochure URL", "url", "Uploaded PDF address"],
      ["fileLabel", "Download label", "text", "e.g. 2026 Prospectus"],
      ["fileSize", "File size", "text", "e.g. PDF · 4.2 MB"]
    ],
    welcome: [
      ["eyebrow", "Eyebrow label", "text", "Short context above the heading"],
      ["imageAlt", "Image description", "text", "Accessible image alternative"]
    ],
    about: [
      ["eyebrow", "Eyebrow label", "text", "e.g. About our institution"],
      ["imageAlt", "Image description", "text", "Accessible image alternative"]
    ],
    principal: [
      ["personName", "Principal name", "text", "Full name"],
      ["designation", "Designation", "text", "Official role"],
      ["quote", "Featured quote", "text", "Short highlighted message"]
    ],
    chairman: [
      ["personName", "Chairman name", "text", "Full name"],
      ["designation", "Designation", "text", "Official role"],
      ["quote", "Featured quote", "text", "Short highlighted message"]
    ],
    why_choose_us: [
      ["intro", "Section introduction", "text", "Why families should choose you"],
      ["columns", "Desktop columns", "number", "Recommended: 3 or 4"]
    ],
    footer_cta: [
      ["heading", "CTA heading", "text", "Final conversion message"],
      ["supportingText", "Supporting text", "text", "One sentence supporting the CTA"],
      ["secondaryButton", "Secondary button", "text", "Optional secondary action"]
    ]
  };

  const collectionSchemas = {
    hero: [["imageUrl", "Image"], ["alt", "Image description"]],
    courses: [["title", "Course title"], ["description", "Description"], ["url", "Page URL"], ["imageUrl", "Image URL"]],
    departments: [["title", "Department"], ["description", "Description"], ["url", "Page URL"], ["imageUrl", "Image URL"]],
    why_choose_us: [["title", "Reason"], ["description", "Description"], ["icon", "Icon name"]],
    announcements: [["title", "Announcement"], ["date", "Date"], ["url", "Link"], ["summary", "Summary"]],
    latest_news: [["title", "News title"], ["date", "Date"], ["url", "Link"], ["imageUrl", "Image URL"]],
    upcoming_events: [["title", "Event title"], ["date", "Date"], ["url", "Link"], ["location", "Location"]],
    gallery: [["title", "Image title"], ["imageUrl", "Image URL"], ["alt", "Image description"]],
    testimonials: [["name", "Person name"], ["role", "Role / relation"], ["quote", "Testimonial"], ["imageUrl", "Portrait URL"]],
    achievements: [["title", "Achievement"], ["year", "Year"], ["description", "Description"], ["imageUrl", "Image URL"]],
    partners: [["name", "Partner name"], ["logoUrl", "Logo URL"], ["url", "Website URL"]]
  };

  window.initHomePageEditor = function (options) {
    const initialHtml = typeof options === "string" ? options : options?.description || "";
    const sectionKey = typeof options === "object" ? options.sectionKey : "";
    const editorHost = $("#editor");
    const descriptionField = $("#description-field");
    let quill;

    if (editorHost && descriptionField && typeof Quill !== "undefined") {
      quill = new Quill("#editor", {
        theme: "snow",
        placeholder: "Write clear, engaging copy for this section…",
        modules: {
          toolbar: [
            [{ header: [2, 3, false] }],
            ["bold", "italic", "underline"],
            [{ list: "ordered" }, { list: "bullet" }],
            ["blockquote", "link"],
            ["clean"]
          ]
        }
      });
      if (initialHtml) quill.root.innerHTML = initialHtml;
    } else if (editorHost && descriptionField) {
      // The rich-text editor could not be loaded. Without this the description is an empty box
      // above a hidden field: nothing to type into, and no way to tell why. A plain text area
      // keeps the section editable — formatting is the only thing lost.
      const fallback = document.createElement("textarea");
      fallback.className = "editor-fallback";
      fallback.rows = 10;
      fallback.value = initialHtml;
      fallback.placeholder = "Write clear, engaging copy for this section…";
      fallback.addEventListener("input", function () {
        descriptionField.value = fallback.value;
      });
      editorHost.replaceChildren(fallback);
      descriptionField.value = initialHtml;
    }

    const jsonField = $("#json-config");
    const builder = $("#config-builder");
    let config = {};
    try { config = JSON.parse(jsonField?.value || "{}"); } catch { config = {}; }

    // Offered on every section, whatever else it can be configured with — a school should not
    // have to know which sections happen to have a field list to choose how one arrives.
    const animationChoices = [
      ["", "Default (fade up)"],
      ["fade", "Fade only"],
      ["fade-down", "Fade down"],
      ["zoom", "Zoom in"],
      ["slide-left", "Slide from left"],
      ["slide-right", "Slide from right"],
      ["rise", "Rise up"],
      ["blur", "Sharpen into focus"],
      ["none", "No animation"]
    ];

    const backdropChoices = [
      ["", "None"],
      ["dots", "Dots"],
      ["grid", "Grid"],
      ["diagonal", "Diagonal lines"],
      ["rings", "Rings"],
      ["waves", "Waves"],
      ["drift", "Drifting dots (moving)"],
      ["bubbles", "Rising bubbles (moving)"],
      ["shimmer", "Shimmer sweep (moving)"],
      ["twinkle", "Twinkling stars (moving)"]
    ];

    function appendChoiceField(host, key, title, help, choices) {
      const field = document.createElement("label");
      field.className = "field";
      const label = document.createElement("span");
      label.innerHTML = `${title}<small>${help}</small>`;
      const select = document.createElement("select");
      select.dataset.configKey = key;
      choices.forEach(([value, text]) => {
        const option = document.createElement("option");
        option.value = value;
        option.textContent = text;
        select.append(option);
      });
      select.value = config[key] ?? "";
      field.append(label, select);
      host.append(field);
      select.addEventListener("change", syncConfig);
    }

    function appendAnimationField(host) {
      const field = document.createElement("label");
      field.className = "field";
      const title = document.createElement("span");
      title.innerHTML = "Entrance animation<small>How this section arrives when scrolled to</small>";
      const select = document.createElement("select");
      select.dataset.configKey = "animation";
      animationChoices.forEach(([value, label]) => {
        const option = document.createElement("option");
        option.value = value;
        option.textContent = label;
        select.append(option);
      });
      select.value = config.animation ?? "";
      field.append(title, select);
      host.append(field);
      select.addEventListener("change", syncConfig);
    }

    const schema = sectionSchemas[sectionKey] || [];
    if (builder) {
      const collectionSchema = collectionSchemas[sectionKey];
      if (schema.length === 0 && !collectionSchema) {
        builder.innerHTML = '<div class="field full"><span class="field-help">This section uses an advanced flexible configuration. Edit the JSON below when additional structured content is required.</span></div>';
        appendAnimationField(builder);
        appendChoiceField(builder, "background", "Background pattern",
          "Drawn on the page — costs nothing to load. Sits behind this section's text.",
          backdropChoices);
      } else {
        schema.forEach(([key, label, type, help]) => {
          const field = document.createElement("label");
          field.className = "field";
          const title = document.createElement("span");
          title.innerHTML = `${label}<small>${help}</small>`;
          const input = document.createElement("input");
          input.type = type;
          input.value = config[key] ?? "";
          input.dataset.configKey = key;
          field.append(title, input);
          builder.append(field);
          input.addEventListener("input", syncConfig);
        });

        appendAnimationField(builder);
        appendChoiceField(builder, "background", "Background pattern",
          "Drawn on the page — costs nothing to load. Sits behind this section's text.",
          backdropChoices);

        if (collectionSchema) {
          const collection = document.createElement("div");
          collection.className = "config-collection";
          collection.innerHTML = '<div class="config-collection__header"><span><strong>Content items</strong><small>Add, edit or remove repeatable cards.</small></span><button type="button" class="btn btn-secondary btn-sm" data-add-item>+ Add item</button></div><div data-items></div>';
          builder.append(collection);
          const itemsHost = $("[data-items]", collection);

          const renderItems = () => {
            itemsHost.innerHTML = "";
            const items = Array.isArray(config.items) ? config.items : [];
            items.forEach((item, itemIndex) => {
              const card = document.createElement("div");
              card.className = "config-item";
              const heading = document.createElement("div");
              heading.className = "config-item__header";
              heading.innerHTML = `<strong>Item ${itemIndex + 1}</strong><button type="button" title="Remove item" data-remove-item="${itemIndex}">×</button>`;
              card.append(heading);
              const fields = document.createElement("div");
              fields.className = "config-item__fields";
              collectionSchema.forEach(([key, label]) => {
                const field = document.createElement("label");
                field.className = "field";
                const caption = document.createElement("span");
                caption.textContent = label;
                const input = key === "description" || key === "summary" || key === "quote"
                  ? document.createElement("textarea")
                  : document.createElement("input");
                input.value = item[key] ?? "";
                input.dataset.itemIndex = String(itemIndex);
                input.dataset.itemKey = key;
                field.append(caption);

                if (/url$/i.test(key)) {
                  // The picker fills the field beside it, so a picture is chosen from the media
                  // library — or uploaded there and then — instead of an address being typed.
                  const row = document.createElement("div");
                  row.className = "media-url-row";
                  const browse = document.createElement("button");
                  browse.type = "button";
                  browse.className = "btn btn-secondary btn-sm";
                  browse.textContent = "Browse";
                  browse.setAttribute("data-media-picker", "");
                  row.append(input, browse);
                  field.append(row);
                } else {
                  field.append(input);
                }

                fields.append(field);
              });
              card.append(fields);
              itemsHost.append(card);
            });
          };

          collection.addEventListener("change", (event) => {
            const picked = event.target.closest("[data-item-key]");
            if (!picked) return;
            config.items[Number(picked.dataset.itemIndex)][picked.dataset.itemKey] = picked.value;
            if (jsonField) jsonField.value = JSON.stringify(config, null, 2);
          });
          collection.addEventListener("input", (event) => {
            const input = event.target.closest("[data-item-key]");
            if (!input) return;
            const index = Number(input.dataset.itemIndex);
            config.items[index][input.dataset.itemKey] = input.value;
            if (jsonField) jsonField.value = JSON.stringify(config, null, 2);
          });
          collection.addEventListener("click", (event) => {
            if (event.target.closest("[data-add-item]")) {
              config.items = Array.isArray(config.items) ? config.items : [];
              config.items.push({});
              renderItems();
              if (jsonField) jsonField.value = JSON.stringify(config, null, 2);
            }
            const remove = event.target.closest("[data-remove-item]");
            if (remove) {
              config.items.splice(Number(remove.dataset.removeItem), 1);
              renderItems();
              if (jsonField) jsonField.value = JSON.stringify(config, null, 2);
            }
          });
          config.items = Array.isArray(config.items) ? config.items : [];
          renderItems();
        }
      }
    }

    function syncConfig() {
      $$("[data-config-key]", builder).forEach((input) => {
        config[input.dataset.configKey] = input.tagName !== "SELECT" && input.type === "number"
          ? Number(input.value || 0)
          : input.value;
      });
      if (jsonField) jsonField.value = JSON.stringify(config, null, 2);
      updateMiniPreview();
    }

    jsonField?.addEventListener("input", () => {
      try {
        config = JSON.parse(jsonField.value || "{}");
        $$("[data-config-key]", builder).forEach((input) => {
          input.value = config[input.dataset.configKey] ?? "";
        });
        jsonField.setCustomValidity("");
      } catch {
        jsonField.setCustomValidity("Enter valid JSON.");
      }
      updateMiniPreview();
    });

    const title = $("#section-title");
    const subtitle = $("#section-subtitle");
    const button = $("#section-button");
    [title, subtitle, button].forEach((input) => input?.addEventListener("input", updateMiniPreview));
    quill?.on("text-change", updateMiniPreview);

    function updateMiniPreview() {
      const heading = $("#mini-preview-title");
      const copy = $("#mini-preview-copy");
      const cta = $("#mini-preview-button");
      if (heading) heading.textContent = title?.value || config.heading || "Your section heading";
      if (copy) copy.textContent = subtitle?.value || config.description || quill?.getText().trim().slice(0, 100) || "Supporting content will appear here.";
      if (cta) cta.textContent = button?.value || config.primaryButton || "Learn more";
    }

    const form = $("#section-form");
    form?.addEventListener("submit", () => {
      if (quill && descriptionField) descriptionField.value = quill.root.innerHTML;
      syncConfig();
    });
    updateMiniPreview();
  };
})();

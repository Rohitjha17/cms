(function () {
  const schemas = {
    About: {
      fields: [
        { key: "mission", label: "Mission", type: "textarea" },
        { key: "vision", label: "Vision", type: "textarea" },
        { key: "history", label: "History", type: "textarea" }
      ]
    },
    Admission: {
      fields: [
        { key: "eligibility", label: "Eligibility", type: "textarea" }
      ],
      collections: [
        {
          key: "processSteps",
          label: "Admission process steps",
          itemFields: [
            { key: "title", label: "Step title", type: "text" },
            { key: "description", label: "Description", type: "textarea" }
          ]
        },
        {
          key: "documents",
          label: "Required documents",
          itemFields: [{ key: "value", label: "Document name", type: "text" }],
          scalar: true
        }
      ]
    },
    Facilities: {
      collections: [
        {
          key: "items",
          label: "Facilities",
          itemFields: [
            { key: "title", label: "Title", type: "text" },
            { key: "description", label: "Description", type: "textarea" },
            { key: "imageUrl", label: "Image URL", type: "media", media: "Image" }
          ]
        }
      ]
    },
    Messages: {
      collections: [
        {
          key: "messages",
          label: "Leadership messages",
          itemFields: [
            { key: "role", label: "Role (Principal/Manager/Director)", type: "text" },
            { key: "name", label: "Name", type: "text" },
            { key: "photoUrl", label: "Photo URL", type: "media", media: "Image" },
            { key: "message", label: "Message", type: "textarea" }
          ]
        }
      ]
    },
    Gallery: {
      collections: [
        {
          key: "items",
          label: "Gallery media",
          itemFields: [
            { key: "album", label: "Album name", type: "text" },
            { key: "type", label: "Type", type: "select", options: ["image", "video"] },
            { key: "url", label: "Image URL or video embed URL", type: "media", media: "Image" },
            { key: "caption", label: "Caption", type: "text" }
          ]
        }
      ]
    },
    Disclosure: {
      collections: [
        {
          key: "documents",
          label: "Mandatory disclosure documents",
          itemFields: [
            { key: "title", label: "Document title", type: "text" },
            { key: "category", label: "Category", type: "text" },
            { key: "fileUrl", label: "PDF URL", type: "media", media: "Document" }
          ]
        }
      ]
    },
    Committee: {
      collections: [
        {
          key: "members",
          label: "Committee members",
          itemFields: [
            { key: "name", label: "Name", type: "text" },
            { key: "role", label: "Role", type: "text" },
            { key: "photoUrl", label: "Photo URL", type: "media", media: "Image" }
          ]
        }
      ]
    },
    Contact: {
      fields: [
        { key: "intro", label: "Intro text", type: "textarea" },
        { key: "formEnabled", label: "Enable enquiry form", type: "checkbox" }
      ]
    }
  };

  function parseJson(value) {
    try { return value ? JSON.parse(value) : {}; }
    catch { return {}; }
  }

  function fieldHtml(field, value, path) {
    const id = `typed-${path.replace(/[^a-z0-9]/gi, "-")}`;
    if (field.type === "textarea") {
      return `<label class="field full"><span>${field.label}</span><textarea data-typed-path="${path}" id="${id}" rows="3">${value ?? ""}</textarea></label>`;
    }
    if (field.type === "checkbox") {
      return `<label class="switch-row full"><input type="checkbox" data-typed-path="${path}" ${value ? "checked" : ""} /><span><strong>${field.label}</strong></span></label>`;
    }
    if (field.type === "select") {
      const options = (field.options || []).map((option) =>
        `<option value="${option}" ${value === option ? "selected" : ""}>${option}</option>`).join("");
      return `<label class="field"><span>${field.label}</span><select data-typed-path="${path}" id="${id}">${options}</select></label>`;
    }
    if (field.type === "media") {
      return `<label class="field full"><span>${field.label}</span>
        <div class="media-url-row">
          <input data-typed-path="${path}" id="${id}" value="${value ?? ""}" />
          <button type="button" class="btn btn-secondary btn-sm" data-media-picker="#${id}">Browse</button>
        </div></label>`;
    }
    return `<label class="field"><span>${field.label}</span><input data-typed-path="${path}" id="${id}" value="${value ?? ""}" /></label>`;
  }

  function collectionHtml(collection, items, basePath) {
    const rows = (items || []).map((item, index) => {
      if (collection.scalar) {
        const value = typeof item === "string" ? item : item?.value || "";
        return `<div class="config-item" data-typed-item>
          ${fieldHtml(collection.itemFields[0], value, `${basePath}.${index}`)}
          <button type="button" class="btn btn-secondary btn-sm" data-typed-remove>Remove</button>
        </div>`;
      }
      return `<div class="config-item" data-typed-item>
        ${collection.itemFields.map((field) => {
          if (field.type === "nested") {
            return `<div class="config-collection" data-typed-nested="${field.key}" data-nested-path="${basePath}.${index}.${field.key}">
              <strong>${field.label}</strong>
              ${(item[field.key] || []).map((nested, nIndex) => `
                <div class="config-item" data-typed-item>
                  ${field.itemFields.map((nf) => fieldHtml(nf, nested[nf.key] ?? "", `${basePath}.${index}.${field.key}.${nIndex}.${nf.key}`)).join("")}
                  <button type="button" class="btn btn-secondary btn-sm" data-typed-remove>Remove</button>
                </div>`).join("")}
              <button type="button" class="btn btn-secondary btn-sm" data-typed-add-nested data-nested-fields='${JSON.stringify(field.itemFields)}'>Add media item</button>
            </div>`;
          }
          return fieldHtml(field, item[field.key] ?? "", `${basePath}.${index}.${field.key}`);
        }).join("")}
        <button type="button" class="btn btn-secondary btn-sm" data-typed-remove>Remove</button>
      </div>`;
    }).join("");

    return `<div class="config-collection" data-typed-collection="${collection.key}" data-scalar="${collection.scalar ? "true" : "false"}">
      <strong>${collection.label}</strong>
      <div data-typed-items>${rows}</div>
      <button type="button" class="btn btn-secondary btn-sm" data-typed-add data-fields='${JSON.stringify(collection.itemFields)}' data-scalar="${collection.scalar ? "true" : "false"}">Add item</button>
    </div>`;
  }

  function setByPath(obj, path, value) {
    const parts = path.split(".");
    let cursor = obj;
    for (let i = 0; i < parts.length - 1; i += 1) {
      const part = parts[i];
      const next = parts[i + 1];
      const isIndex = String(Number(next)) === next;
      if (cursor[part] == null) cursor[part] = isIndex ? [] : {};
      cursor = cursor[part];
    }
    const last = parts[parts.length - 1];
    cursor[last] = value;
  }

  function collect(root) {
    const data = {};
    root.querySelectorAll("[data-typed-path]").forEach((el) => {
      const path = el.dataset.typedPath;
      let value = el.type === "checkbox" ? el.checked : el.value;
      // Convert scalar document arrays like documents.0 -> documents: ["..."]
      if (/^documents\.\d+$/.test(path)) {
        const index = Number(path.split(".")[1]);
        data.documents = data.documents || [];
        data.documents[index] = value;
        return;
      }
      setByPath(data, path, value);
    });
    // Compact sparse arrays
    Object.keys(data).forEach((key) => {
      if (Array.isArray(data[key])) {
        data[key] = data[key].filter((item) => item !== undefined);
      }
    });
    return data;
  }

  function init() {
    const host = document.querySelector("[data-typed-editor]");
    const jsonInput = document.querySelector("[data-typed-json]");
    const typeSelect = document.querySelector("[data-page-type]");
    if (!host || !jsonInput || !typeSelect) return;

    const normalizeData = (type, data) => {
      // Flatten legacy nested gallery albums into simple media rows.
      if (type === "Gallery" && Array.isArray(data.albums) && !Array.isArray(data.items)) {
        data.items = data.albums.flatMap((album) =>
          (album.items || []).map((item) => ({
            album: album.title || "Gallery",
            type: item.type || "image",
            url: item.url || "",
            caption: item.caption || ""
          })));
      }
      return data;
    };

    const render = () => {
      const type = typeSelect.value;
      const schema = schemas[type];
      const data = normalizeData(type, parseJson(jsonInput.value));
      if (!schema) {
        host.innerHTML = `<p class="muted">Custom pages use the HTML content field. Structured JSON is optional.</p>`;
        return;
      }
      host.innerHTML = [
        `<p class="muted" style="margin:0 0 1rem">Fill the fields below. Use Browse to pick images/PDFs from the media library.</p>`,
        ...(schema.fields || []).map((field) => fieldHtml(field, data[field.key], field.key)),
        ...(schema.collections || []).map((collection) =>
          collectionHtml(collection, data[collection.key] || [], collection.key))
      ].join("");
    };

    const sync = () => {
      jsonInput.value = JSON.stringify(collect(host), null, 2);
    };

    host.addEventListener("input", sync);
    host.addEventListener("change", sync);
    host.addEventListener("click", (event) => {
      if (event.target.closest("[data-typed-remove]")) {
        event.target.closest("[data-typed-item]")?.remove();
        sync();
        return;
      }
      const add = event.target.closest("[data-typed-add]");
      if (add) {
        const container = add.parentElement.querySelector("[data-typed-items]");
        const fields = JSON.parse(add.dataset.fields || "[]");
        const scalar = add.dataset.scalar === "true";
        const index = container.children.length;
        const collectionKey = add.closest("[data-typed-collection]")?.dataset.typedCollection;
        const wrapper = document.createElement("div");
        wrapper.className = "config-item";
        wrapper.dataset.typedItem = "true";
        if (scalar) {
          wrapper.innerHTML = `${fieldHtml(fields[0], "", `${collectionKey}.${index}`)}
            <button type="button" class="btn btn-secondary btn-sm" data-typed-remove>Remove</button>`;
        } else {
          wrapper.innerHTML = `${fields.map((field) => {
            if (field.type === "nested") {
              return `<div class="config-collection" data-typed-nested="${field.key}" data-nested-path="${collectionKey}.${index}.${field.key}">
                <strong>${field.label}</strong>
                <div data-typed-items></div>
                <button type="button" class="btn btn-secondary btn-sm" data-typed-add-nested data-nested-fields='${JSON.stringify(field.itemFields)}'>Add media item</button>
              </div>`;
            }
            return fieldHtml(field, "", `${collectionKey}.${index}.${field.key}`);
          }).join("")}
          <button type="button" class="btn btn-secondary btn-sm" data-typed-remove>Remove</button>`;
        }
        container.appendChild(wrapper);
        sync();
        return;
      }
      const addNested = event.target.closest("[data-typed-add-nested]");
      if (addNested) {
        const nestedRoot = addNested.closest("[data-typed-nested]");
        const fields = JSON.parse(addNested.dataset.nestedFields || "[]");
        const base = nestedRoot.dataset.nestedPath;
        const items = nestedRoot.querySelector("[data-typed-items]") || nestedRoot;
        const index = items.querySelectorAll(":scope > [data-typed-item]").length;
        const wrapper = document.createElement("div");
        wrapper.className = "config-item";
        wrapper.dataset.typedItem = "true";
        wrapper.innerHTML = `${fields.map((field) => fieldHtml(field, "", `${base}.${index}.${field.key}`)).join("")}
          <button type="button" class="btn btn-secondary btn-sm" data-typed-remove>Remove</button>`;
        (nestedRoot.querySelector("[data-typed-items]") || nestedRoot).insertBefore(wrapper, addNested);
        sync();
      }
    });

    typeSelect.addEventListener("change", render);
    const form = host.closest("form");
    form?.addEventListener("submit", sync);
    render();
  }

  document.addEventListener("DOMContentLoaded", init);
})();

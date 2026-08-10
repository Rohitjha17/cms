/* Live SEO preview: mirrors the form into the search-result and social cards, and
   colours the character counters against the limits Google truncates at. */
(function () {
  "use strict";

  var LIMITS = { title: [30, 60], description: [70, 160] };

  var fields = {
    title: document.querySelector("[data-seo-title]"),
    description: document.querySelector("[data-seo-description]"),
    image: document.querySelector("[data-seo-image]")
  };

  var fallback = {
    title: document.querySelector('[data-preview="title"]')?.textContent || "",
    description: document.querySelector('[data-preview="description"]')?.textContent || ""
  };

  function paintCounter(name, length) {
    var counter = document.querySelector('[data-counter-for="' + name + '"]');
    if (!counter) return;
    counter.textContent = String(length);

    var range = LIMITS[name];
    counter.classList.remove("is-warn", "is-over");
    if (length === 0) return;
    if (length > range[1]) counter.classList.add("is-over");
    else if (length < range[0]) counter.classList.add("is-warn");
  }

  function paintText(name) {
    var field = fields[name];
    if (!field) return;
    var value = field.value.trim();
    paintCounter(name, value.length);

    document.querySelectorAll('[data-preview="' + name + '"]').forEach(function (node) {
      node.textContent = value || fallback[name];
    });
  }

  function paintImage() {
    var holder = document.querySelector('[data-preview="image"]');
    if (!holder || !fields.image) return;
    var url = fields.image.value.trim();
    holder.innerHTML = url
      ? '<img alt="">'
      : "<span>No sharing image</span>";
    if (url) holder.querySelector("img").src = url;
  }

  ["title", "description"].forEach(function (name) {
    if (!fields[name]) return;
    fields[name].addEventListener("input", function () { paintText(name); });
    paintText(name);
  });

  if (fields.image) {
    fields.image.addEventListener("input", paintImage);
    fields.image.addEventListener("change", paintImage);
  }
})();

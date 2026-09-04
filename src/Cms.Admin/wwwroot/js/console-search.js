/* Search the console.
   ------------------------------------------------------------------------------------------
   Everything here is one keystroke away: "/" or Ctrl-K opens it, typing narrows it, Enter goes
   there. The list is built on the server from the same constants the sidebar and the section
   editor use, so a section added to the website is findable the day it is added rather than the
   day somebody remembers to update a second list. */
(function () {
  "use strict";

  var payload = document.querySelector("[data-search-index]");
  var panel = document.querySelector("[data-search-panel]");
  if (!payload || !panel) return;

  var entries;
  try { entries = JSON.parse(payload.textContent) || []; } catch (e) { return; }

  var input = panel.querySelector("[data-search-input]");
  var list = panel.querySelector("[data-search-results]");
  var empty = panel.querySelector("[data-search-empty]");
  var at = 0;
  var shown = [];

  function score(entry, term) {
    var label = entry.label.toLowerCase();
    if (label === term) return 0;
    if (label.indexOf(term) === 0) return 1;
    if (label.indexOf(term) > 0) return 2;
    if ((entry.where || "").toLowerCase().indexOf(term) >= 0) return 3;
    if ((entry.keywords || "").indexOf(term) >= 0) return 4;
    return -1;
  }

  function render() {
    var term = input.value.trim().toLowerCase();
    shown = (term
      ? entries
          .map(function (e) { return { e: e, s: score(e, term) }; })
          .filter(function (x) { return x.s >= 0; })
          .sort(function (a, b) { return a.s - b.s; })
          .map(function (x) { return x.e; })
      : entries
    ).slice(0, 12);

    at = 0;
    list.textContent = "";
    shown.forEach(function (entry, index) {
      var item = document.createElement("li");
      var link = document.createElement("a");
      link.href = entry.href;
      link.setAttribute("role", "option");
      link.innerHTML = "<strong></strong><span></span>";
      link.querySelector("strong").textContent = entry.label;
      link.querySelector("span").textContent = entry.where;
      if (index === 0) { link.classList.add("is-active"); }
      item.appendChild(link);
      list.appendChild(item);
    });

    empty.hidden = shown.length > 0;
  }

  function highlight() {
    var links = list.querySelectorAll("a");
    links.forEach(function (link, index) { link.classList.toggle("is-active", index === at); });
    if (links[at]) { links[at].scrollIntoView({ block: "nearest" }); }
  }

  function open() {
    panel.hidden = false;
    document.body.classList.add("is-searching");
    input.value = "";
    render();
    input.focus();
  }

  function close() {
    panel.hidden = true;
    document.body.classList.remove("is-searching");
  }

  document.addEventListener("click", function (event) {
    if (event.target.closest("[data-search-open]")) { event.preventDefault(); open(); }
    else if (event.target.closest("[data-search-close]")) { close(); }
  });

  document.addEventListener("keydown", function (event) {
    var typing = /^(input|textarea|select)$/i.test(event.target.tagName) || event.target.isContentEditable;

    if (!panel.hidden) {
      if (event.key === "Escape") { close(); }
      else if (event.key === "ArrowDown") { event.preventDefault(); at = Math.min(at + 1, shown.length - 1); highlight(); }
      else if (event.key === "ArrowUp") { event.preventDefault(); at = Math.max(at - 1, 0); highlight(); }
      else if (event.key === "Enter" && shown[at]) { event.preventDefault(); window.location.href = shown[at].href; }
      return;
    }

    // "/" is a shortcut only when it is not being typed into something.
    if ((event.key === "k" || event.key === "K") && (event.metaKey || event.ctrlKey)) { event.preventDefault(); open(); }
    else if (event.key === "/" && !typing && !event.metaKey && !event.ctrlKey) { event.preventDefault(); open(); }
  });

  input.addEventListener("input", render);
})();

/* A settings panel reached from the search is marked, so the eye lands on it instead of
   searching a long page for whatever it was that just scrolled past. */
(function () {
  "use strict";

  var target = window.location.hash && document.querySelector(window.location.hash);
  if (!target) return;

  target.classList.add("is-found");
  window.setTimeout(function () { target.classList.remove("is-found"); }, 2200);
})();

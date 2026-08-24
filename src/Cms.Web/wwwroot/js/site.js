/* Public website interaction layer.
 *
 * Progressive enhancement only: every page is fully readable and navigable with this
 * file absent or blocked. Nothing here changes content or behaviour, it only adds
 * scroll/press feedback — and all of it stands down when the visitor has asked for
 * reduced motion.
 */
(function () {
  "use strict";

  var prefersReducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

  /* ---------------------------------------------------------------- Mobile nav */

  var navToggle = document.querySelector("[data-nav-toggle]");
  var nav = document.querySelector("[data-primary-nav]");

  function setNav(open) {
    if (!nav || !navToggle) return;
    nav.classList.toggle("is-open", open);
    navToggle.setAttribute("aria-expanded", String(open));
    navToggle.textContent = open ? "Close" : "Menu";
    document.body.classList.toggle("nav-locked", open);
  }

  if (navToggle && nav) {
    navToggle.addEventListener("click", function () {
      setNav(!nav.classList.contains("is-open"));
    });

    // Closing on Escape and on navigation keeps the drawer from trapping the visitor.
    document.addEventListener("keydown", function (event) {
      if (event.key === "Escape" && nav.classList.contains("is-open")) {
        setNav(false);
        navToggle.focus();
      }
    });

    nav.addEventListener("click", function (event) {
      if (event.target.closest("a")) setNav(false);
    });

    window.addEventListener("resize", function () {
      if (window.innerWidth > 1000 && nav.classList.contains("is-open")) setNav(false);
    });
  }

  /* ------------------------------------------------------- Header scroll state */

  var header = document.querySelector(".site-header");
  var backToTop = null;

  function onScroll() {
    var y = window.scrollY || document.documentElement.scrollTop;
    if (header) header.classList.toggle("is-scrolled", y > 12);
    if (backToTop) backToTop.classList.toggle("is-visible", y > 640);
  }

  /* ------------------------------------------------------------- Back to top */

  if (document.querySelector(".site-footer")) {
    backToTop = document.createElement("button");
    backToTop.type = "button";
    backToTop.className = "to-top";
    backToTop.setAttribute("aria-label", "Back to top");
    backToTop.innerHTML =
      '<svg viewBox="0 0 24 24" aria-hidden="true" fill="none" stroke="currentColor" ' +
      'stroke-width="2" stroke-linecap="round" stroke-linejoin="round">' +
      '<path d="M12 19V5M5 12l7-7 7 7"/></svg>';
    backToTop.addEventListener("click", function () {
      window.scrollTo({ top: 0, behavior: prefersReducedMotion ? "auto" : "smooth" });
    });
    document.body.appendChild(backToTop);
  }

  window.addEventListener("scroll", onScroll, { passive: true });
  onScroll();

  /* --------------------------------------------------------- Reveal on scroll */

  // Selected in JS rather than marked up in Razor so the views stay presentation-free.
  var REVEAL = [
    ".page-body > *",
    ".section > *",
    ".stats-grid .stat",
    ".card-grid > article",
    ".split-cards article",
    ".campus-panels article",
    ".academic-columns article",
    ".people-card",
    ".news-item",
    ".event-card",
    ".message-list article",
    ".gallery-grid figure",
    ".cta-band",
    ".steps li"
  ].join(",");

  if (!prefersReducedMotion && "IntersectionObserver" in window) {
    // Anything already on screen is left untouched: animating it would mean hiding
    // content the visitor can already see, then fading it back in.
    var viewportHeight = window.innerHeight || document.documentElement.clientHeight;
    var targets = Array.prototype.slice
      .call(document.querySelectorAll(REVEAL))
      .filter(function (el) { return el.getBoundingClientRect().top > viewportHeight * 0.9; });

    // Stagger siblings so a grid resolves as a sequence rather than one block.
    var lastParent = null;
    var indexInParent = 0;
    targets.forEach(function (el) {
      if (el.parentElement !== lastParent) {
        lastParent = el.parentElement;
        indexInParent = 0;
      }
      el.style.setProperty("--reveal-delay", Math.min(indexInParent, 6) * 55 + "ms");
      indexInParent += 1;
      el.classList.add("reveal");
    });

    var observer = new IntersectionObserver(function (entries) {
      entries.forEach(function (entry) {
        if (!entry.isIntersecting) return;
        entry.target.classList.add("is-revealed");
        observer.unobserve(entry.target);
      });
    }, { rootMargin: "0px 0px -8% 0px", threshold: 0.06 });

    targets.forEach(function (el) { observer.observe(el); });

    // Safety net: if the observer never fires for any reason, nothing stays hidden.
    window.setTimeout(function () {
      targets.forEach(function (el) { el.classList.add("is-revealed"); });
    }, 3000);
  }

  /* ------------------------------------------------------------- Image fade-in */

  Array.prototype.forEach.call(document.images, function (img) {
    if (img.complete) {
      img.classList.add("is-loaded");
    } else {
      img.addEventListener("load", function () { img.classList.add("is-loaded"); });
      img.addEventListener("error", function () { img.classList.add("is-loaded"); });
    }
  });
})();

/* Hero slideshow: moves on its own, and by the arrows or the dots. It pauses while the
   pointer is over it and while the tab is hidden, and it does not move at all for a visitor
   who has asked for reduced motion. */
(function () {
  var carousels = document.querySelectorAll("[data-hero-carousel]");
  if (!carousels.length) return;

  var stillness = window.matchMedia("(prefers-reduced-motion: reduce)");

  carousels.forEach(function (carousel) {
    var slides = Array.prototype.slice.call(carousel.querySelectorAll(".hero-slides__item"));
    var dots = Array.prototype.slice.call(carousel.querySelectorAll("[data-hero-dot]"));
    if (slides.length < 2) return;

    var current = 0;
    var timer = null;
    var seconds = parseFloat(carousel.getAttribute("data-autoplay")) || 6;

    function show(next) {
      current = (next + slides.length) % slides.length;
      slides.forEach(function (slide, index) {
        slide.classList.toggle("is-active", index === current);
      });
      dots.forEach(function (dot, index) {
        dot.classList.toggle("is-active", index === current);
      });
    }

    function start() {
      stop();
      if (stillness.matches || seconds <= 0) return;
      timer = window.setInterval(function () { show(current + 1); }, seconds * 1000);
    }

    function stop() {
      if (timer) { window.clearInterval(timer); timer = null; }
    }

    carousel.querySelector("[data-hero-next]")?.addEventListener("click", function () { show(current + 1); start(); });
    carousel.querySelector("[data-hero-prev]")?.addEventListener("click", function () { show(current - 1); start(); });
    dots.forEach(function (dot, index) {
      dot.addEventListener("click", function () { show(index); start(); });
    });

    carousel.addEventListener("mouseenter", stop);
    carousel.addEventListener("mouseleave", start);
    document.addEventListener("visibilitychange", function () {
      if (document.hidden) { stop(); } else { start(); }
    });
    stillness.addEventListener?.("change", start);

    start();
  });
})();

/* Condenses the header once the page is scrolled. Purely cosmetic: if this never runs the
   header simply stays at its full height. */
(function () {
  var header = document.querySelector(".site-header");
  if (!header) return;

  var condensed = false;
  function update() {
    var shouldCondense = window.scrollY > 40;
    if (shouldCondense === condensed) return;
    condensed = shouldCondense;
    header.classList.toggle("is-condensed", condensed);
  }

  window.addEventListener("scroll", update, { passive: true });
  update();
})();

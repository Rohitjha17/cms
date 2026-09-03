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

  // A school that wants a still page can say so from the console. Reading it here rather than
  // per element means the whole effect is skipped, not applied and then undone.
  var animationsOn = document.body.getAttribute("data-scroll-animations") !== "off";

  // Each section can choose how it arrives. The choice is per section and lives with the
  // section's own content, so it travels with a template rather than being a site-wide switch.
  var sectionAnimations = window.cmsSectionAnimations || {};
  Object.keys(sectionAnimations).forEach(function (key) {
    var host = document.querySelector('[data-section="' + key + '"]');
    if (host && sectionAnimations[key]) host.setAttribute("data-animate", sectionAnimations[key]);
  });

  if (animationsOn && !prefersReducedMotion && "IntersectionObserver" in window) {
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

/* ---------------------------------------------------------------- Opening popup */

(function () {
  "use strict";

  var popup = document.querySelector("[data-site-popup]");
  if (!popup) return;

  // Shown once per visit by default. Anything that cannot be remembered — a private window,
  // storage switched off — must still show the popup rather than throw and leave the page
  // half-initialised, so every read and write is guarded.
  var STORAGE_KEY = "cms-popup-dismissed";
  var oncePerVisit = popup.getAttribute("data-popup-key") === "visit";

  function dismissed() {
    if (!oncePerVisit) return false;
    try { return window.sessionStorage.getItem(STORAGE_KEY) === "1"; } catch (e) { return false; }
  }

  function remember() {
    if (!oncePerVisit) return;
    try { window.sessionStorage.setItem(STORAGE_KEY, "1"); } catch (e) { /* not worth failing over */ }
  }

  function open() {
    popup.hidden = false;
    document.body.classList.add("popup-open");
    var focusable = popup.querySelector("input, button");
    if (focusable) focusable.focus({ preventScroll: true });
  }

  function close() {
    popup.hidden = true;
    document.body.classList.remove("popup-open");
    remember();
  }

  // A form that came back with something to say has to be seen, whether or not the visitor
  // closed the popup earlier in this visit.
  var hasStatus = popup.querySelector(".site-popup__status");

  if (hasStatus || !dismissed()) {
    // A beat after load, so the popup does not fight the page for attention while it paints.
    window.setTimeout(open, hasStatus ? 0 : 900);
  }

  popup.addEventListener("click", function (event) {
    if (event.target.closest("[data-popup-close]")) close();
  });

  document.addEventListener("keydown", function (event) {
    if (event.key === "Escape" && !popup.hidden) close();
  });
})();

/* ------------------------------------------------------------- Gallery lightbox */

(function () {
  "use strict";

  // A photograph in a gallery is a thumbnail, and a thumbnail is not what anyone came to see.
  // Opening it full size is the whole point of a gallery page, and every visitor already expects
  // a click to do it.
  var images = Array.prototype.slice.call(
    document.querySelectorAll(".gallery-grid figure img, .page-gallery img, img[data-lightbox]")
  );
  if (images.length === 0) return;

  var overlay = document.createElement("div");
  overlay.className = "lightbox";
  overlay.hidden = true;
  overlay.innerHTML =
    '<button class="lightbox__close" type="button" aria-label="Close">&times;</button>' +
    '<button class="lightbox__nav lightbox__nav--prev" type="button" aria-label="Previous">&#8249;</button>' +
    '<figure class="lightbox__stage"><img alt="" /><figcaption></figcaption></figure>' +
    '<button class="lightbox__nav lightbox__nav--next" type="button" aria-label="Next">&#8250;</button>';
  document.body.appendChild(overlay);

  var stageImage = overlay.querySelector("img");
  var stageCaption = overlay.querySelector("figcaption");
  var index = 0;
  var opener = null;

  function captionFor(img) {
    var figure = img.closest("figure");
    var caption = figure && figure.querySelector("figcaption");
    return (caption && caption.textContent.trim()) || img.getAttribute("alt") || "";
  }

  function show(next) {
    index = (next + images.length) % images.length;
    var img = images[index];
    stageImage.src = img.currentSrc || img.src;
    stageImage.alt = img.getAttribute("alt") || "";
    var caption = captionFor(img);
    stageCaption.textContent = caption;
    stageCaption.hidden = caption.length === 0;
    // One picture is a picture, not a slideshow.
    overlay.classList.toggle("lightbox--single", images.length < 2);
  }

  function open(at, from) {
    opener = from || null;
    show(at);
    overlay.hidden = false;
    document.body.classList.add("popup-open");
    overlay.querySelector(".lightbox__close").focus({ preventScroll: true });
  }

  function close() {
    overlay.hidden = true;
    document.body.classList.remove("popup-open");
    // Back where the visitor was, so the keyboard does not jump to the top of the page.
    if (opener) opener.focus({ preventScroll: true });
    opener = null;
  }

  images.forEach(function (img, at) {
    img.classList.add("is-zoomable");
    // Keyboard users need a control, not a picture: a bare <img> cannot be tabbed to or pressed.
    if (!img.closest("a")) {
      img.setAttribute("tabindex", "0");
      img.setAttribute("role", "button");
      if (!img.getAttribute("aria-label")) {
        img.setAttribute("aria-label", "View " + (img.getAttribute("alt") || "image") + " full size");
      }
      img.addEventListener("click", function () { open(at, img); });
      img.addEventListener("keydown", function (event) {
        if (event.key === "Enter" || event.key === " ") {
          event.preventDefault();
          open(at, img);
        }
      });
    }
  });

  overlay.addEventListener("click", function (event) {
    if (event.target.closest(".lightbox__close")) return close();
    if (event.target.closest(".lightbox__nav--prev")) return show(index - 1);
    if (event.target.closest(".lightbox__nav--next")) return show(index + 1);
    // A click on the backdrop, not on the picture itself.
    if (!event.target.closest(".lightbox__stage")) close();
  });

  document.addEventListener("keydown", function (event) {
    if (overlay.hidden) return;
    if (event.key === "Escape") close();
    else if (event.key === "ArrowLeft") show(index - 1);
    else if (event.key === "ArrowRight") show(index + 1);
  });
})();

/* ---------------------------------------------------------- Counting statistics */

(function () {
  "use strict";

  var stats = Array.prototype.slice.call(document.querySelectorAll(".stats-grid .stat strong"));
  if (stats.length === 0) return;
  if (document.body.getAttribute("data-scroll-animations") === "off") return;
  if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) return;
  if (!("IntersectionObserver" in window)) return;

  // "1 500+" is a prefix, a number and a suffix, and only the middle should move. Anything that
  // is not that shape — a dash, a word — is left exactly as the school typed it.
  var SHAPE = /^(\D*?)([\d][\d\s,]*)(\D*)$/;

  function countUp(el) {
    var match = SHAPE.exec(el.textContent.trim());
    if (!match) return;

    var prefix = match[1];
    var raw = match[2];
    var suffix = match[3];
    var separator = raw.indexOf(",") >= 0 ? "," : (raw.indexOf(" ") >= 0 ? " " : "");
    var target = parseInt(raw.replace(/[\s,]/g, ""), 10);
    if (!isFinite(target) || target <= 0) return;

    function render(value) {
      var text = String(value);
      if (separator) text = text.replace(/\B(?=(\d{3})+(?!\d))/g, separator);
      el.textContent = prefix + text + suffix;
    }

    var duration = 1100;
    var started = null;
    function frame(now) {
      if (started === null) started = now;
      var progress = Math.min((now - started) / duration, 1);
      // Fast first, settling at the end — a linear count reads like a loading spinner.
      render(Math.round(target * (1 - Math.pow(1 - progress, 3))));
      if (progress < 1) window.requestAnimationFrame(frame);
    }

    render(0);
    window.requestAnimationFrame(frame);
  }

  var observer = new IntersectionObserver(function (entries) {
    entries.forEach(function (entry) {
      if (!entry.isIntersecting) return;
      observer.unobserve(entry.target);
      countUp(entry.target);
    });
  }, { threshold: 0.4 });

  stats.forEach(function (el) { observer.observe(el); });
})();

/* -------------------------------------------------------------- Reading progress */

(function () {
  "use strict";

  if (document.body.getAttribute("data-scroll-animations") === "off") return;
  if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) return;

  var bar = document.createElement("div");
  bar.className = "scroll-progress";
  bar.setAttribute("aria-hidden", "true");
  document.body.appendChild(bar);

  var ticking = false;
  function update() {
    var doc = document.documentElement;
    var scrollable = doc.scrollHeight - doc.clientHeight;
    // A page that does not scroll has no progress to report.
    bar.style.transform = "scaleX(" + (scrollable > 0 ? window.scrollY / scrollable : 0) + ")";
    ticking = false;
  }

  window.addEventListener("scroll", function () {
    if (ticking) return;
    ticking = true;
    window.requestAnimationFrame(update);
  }, { passive: true });

  update();
})();

/* --------------------------------------------------------------- Notice marquee */

(function () {
  "use strict";

  var marquee = document.querySelector("[data-notice-marquee]");
  if (!marquee) return;

  var set = marquee.querySelector(".notice-marquee__set");
  if (!set) return;

  // Two copies looked like the notice had been typed twice whenever the text was shorter than
  // the screen — both were on screen at once. The track needs as many copies as it takes to
  // outrun the screen, and must travel exactly one copy's width for the loop to close unseen.
  function build() {
    marquee.querySelectorAll(".notice-marquee__set:not(:first-child)").forEach(function (extra) {
      extra.remove();
    });

    var width = set.getBoundingClientRect().width;
    if (width < 1) return;

    var copies = Math.ceil((window.innerWidth + width) / width) + 1;
    for (var i = 1; i < copies; i++) {
      var clone = set.cloneNode(true);
      clone.setAttribute("aria-hidden", "true");
      marquee.appendChild(clone);
    }

    marquee.style.setProperty("--notice-set-width", width + "px");
  }

  build();

  // Fonts land after first paint and change the width the loop is measured against.
  if (document.fonts && document.fonts.ready) document.fonts.ready.then(build);

  var resizeTimer;
  window.addEventListener("resize", function () {
    window.clearTimeout(resizeTimer);
    resizeTimer = window.setTimeout(build, 200);
  });
})();

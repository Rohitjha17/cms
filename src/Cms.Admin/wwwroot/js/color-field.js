// Colour fields open the operating system's colour panel.
//
// A hex code typed into a plain box is not how anyone picks a school's colours, and a bare
// swatch on its own gives no way to paste the exact hex a brand guide specifies. So every colour
// field is the pair: a swatch that opens the picker, and the hex beside it. The named input is
// still the one that holds the value and is posted, so nothing about the form changes.
//
// Applied to anything marked data-color-field, anything already declared type="color", and any
// field whose name mentions colour — so a colour added later is picked up without extra wiring.
(function () {
    'use strict';

    // The swatch this script creates is itself an input[type=color]; it must never be treated as
    // a field to enhance, or enhancing one field would create another swatch without end.
    var SELECTOR = ':is(input[data-color-field], input[type="color"], .field input[name*="Color" i])'
        + ':not(.color-field__swatch)';
    var FALLBACK = '#000000';

    /** Expands #abc, tolerates a missing #, and rejects anything that is not a hex colour. */
    function toHex(value) {
        if (!value) return null;
        var text = value.trim().replace(/^#/, '');
        if (/^[0-9a-f]{3}$/i.test(text)) {
            text = text[0] + text[0] + text[1] + text[1] + text[2] + text[2];
        }
        return /^[0-9a-f]{6}$/i.test(text) ? '#' + text.toLowerCase() : null;
    }

    function enhance(input) {
        if (input.dataset.colorFieldReady) return;
        input.dataset.colorFieldReady = 'true';

        // A field declared type="color" cannot show its hex, so it becomes the text half here and
        // the swatch is added alongside, exactly like the fields that were plain boxes.
        if (input.type === 'color') input.type = 'text';
        input.autocomplete = 'off';
        input.spellcheck = false;
        if (!input.placeholder) input.placeholder = '#0f2d5c';

        var row = document.createElement('div');
        row.className = 'color-field';

        var swatch = document.createElement('input');
        swatch.type = 'color';
        swatch.className = 'color-field__swatch';
        swatch.dataset.colorFieldReady = 'true';
        swatch.tabIndex = -1;
        swatch.value = toHex(input.value) || FALLBACK;

        var label = input.closest('label');
        var name = label && label.querySelector('span');
        swatch.setAttribute('aria-label', 'Pick ' + (name ? name.textContent.trim().toLowerCase() : 'a colour'));

        input.parentNode.insertBefore(row, input);
        row.appendChild(swatch);
        row.appendChild(input);

        swatch.addEventListener('input', function () {
            input.value = swatch.value;
            // Anything watching the field — a live preview, a dirty-state flag — sees a real edit.
            input.dispatchEvent(new Event('input', { bubbles: true }));
            input.dispatchEvent(new Event('change', { bubbles: true }));
        });

        input.addEventListener('input', function () {
            var hex = toHex(input.value);
            if (hex) swatch.value = hex;
        });

        // Typing "0f2d5c" or "#ABC" is normalised once the field is left, never while typing.
        input.addEventListener('blur', function () {
            var hex = toHex(input.value);
            if (hex) input.value = hex;
        });
    }

    function enhanceAll(root) {
        (root || document).querySelectorAll(SELECTOR).forEach(enhance);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () { enhanceAll(); });
    } else {
        enhanceAll();
    }

    // Rows added after load — a new navigation link, another template field — are picked up too.
    if (window.MutationObserver) {
        new MutationObserver(function (mutations) {
            mutations.forEach(function (mutation) {
                mutation.addedNodes.forEach(function (node) {
                    if (node.nodeType !== 1) return;
                    if (node.matches && node.matches(SELECTOR)) enhance(node);
                    else enhanceAll(node);
                });
            });
        }).observe(document.documentElement, { childList: true, subtree: true });
    }
})();

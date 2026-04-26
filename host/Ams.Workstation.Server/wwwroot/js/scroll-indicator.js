// Tags any element that scrolls with data-ams-scrolling="true" for ~1.2s after
// the last scroll event. CSS in app.css uses that flag to fade in a minimal
// scrollbar thumb only while scrolling is in progress (or on hover / keyboard
// focus). One capturing listener covers every scrollable element on the page.
(function () {
    'use strict';

    const ATTR = 'data-ams-scrolling';
    const HOLD_MS = 1200;

    const timers = new WeakMap();

    function flag(el) {
        if (!el || !(el instanceof Element)) return;
        el.setAttribute(ATTR, 'true');
        const previous = timers.get(el);
        if (previous) {
            clearTimeout(previous);
        }
        const handle = setTimeout(() => {
            el.removeAttribute(ATTR);
            timers.delete(el);
        }, HOLD_MS);
        timers.set(el, handle);
    }

    function onScroll(event) {
        const target = event.target;
        if (target instanceof Element) {
            flag(target);
        } else if (target instanceof Document) {
            const scrollable = document.scrollingElement;
            if (scrollable) flag(scrollable);
        }
    }

    document.addEventListener('scroll', onScroll, { capture: true, passive: true });
})();

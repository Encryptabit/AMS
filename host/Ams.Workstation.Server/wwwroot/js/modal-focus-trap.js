// Focus trap for modal dialogs. Keeps Tab cycling within the visible modal
// and auto-focuses the first focusable element on activation.
window.modalFocusTrap = (() => {
    let handler = null;
    const SELECTOR = 'select, input, textarea, button, [tabindex]:not([tabindex="-1"])';

    function onKeyDown(e) {
        if (e.key !== 'Tab') return;
        const overlay = document.querySelector('.crx-modal-overlay.visible, .ignore-modal-overlay.visible');
        if (!overlay) return;
        const modal = overlay.firstElementChild;
        if (!modal) return;
        const focusable = [...modal.querySelectorAll(SELECTOR)].filter(el => !el.disabled);
        if (focusable.length === 0) return;
        const first = focusable[0];
        const last = focusable[focusable.length - 1];
        if (e.shiftKey) {
            if (document.activeElement === first) {
                e.preventDefault();
                last.focus();
            }
        } else {
            if (document.activeElement === last) {
                e.preventDefault();
                first.focus();
            }
        }
    }

    return {
        activate(focusSelector) {
            if (handler) return;
            handler = onKeyDown;
            document.addEventListener('keydown', handler, true);
            const overlay = document.querySelector('.crx-modal-overlay.visible, .ignore-modal-overlay.visible');
            if (overlay) {
                const target = focusSelector
                    ? overlay.querySelector(focusSelector)
                    : overlay.querySelector(SELECTOR);
                if (target) target.focus();
            }
        },
        deactivate() {
            if (handler) {
                document.removeEventListener('keydown', handler, true);
                handler = null;
            }
        }
    };
})();

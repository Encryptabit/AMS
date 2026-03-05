// Keyboard shortcuts module for ChapterReview page.
// Communicates with Blazor via DotNetObjectReference JSInvokable calls.

let _dotNetRef = null;
let _handler = null;

function isModalOpen() {
    return document.querySelector('.crx-modal-overlay.visible') !== null;
}

function isInputFocused() {
    const el = document.activeElement;
    if (!el) return false;
    const tag = el.tagName;
    if (tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT') return true;
    if (el.contentEditable === 'true') return true;
    return false;
}

function handleKeydown(e) {
    if (!_dotNetRef) return;

    const modal = isModalOpen();
    const inInput = isInputFocused();

    // When in an input field, only allow Enter to submit modal
    if (inInput) {
        if (!modal) return;

        if (e.key === 'Enter' && !e.shiftKey && document.activeElement.tagName !== 'TEXTAREA') {
            _dotNetRef.invokeMethodAsync('OnModalSubmit');
            e.preventDefault();
            return;
        }

        return;
    }

    // Modal open (not in input)
    if (modal) {
        if ((e.key === 'q' || e.key === 'Q') && !e.ctrlKey && !e.metaKey && !e.altKey) {
            _dotNetRef.invokeMethodAsync('OnModalClose');
            e.preventDefault();
            return;
        }

        if (e.key === 'Enter' && !e.shiftKey) {
            _dotNetRef.invokeMethodAsync('OnModalSubmit');
            e.preventDefault();
            return;
        }

        // All other keys ignored when modal is open
        return;
    }

    // Modal NOT open, NOT in input field
    if (e.key === 'ArrowRight' && e.altKey) {
        _dotNetRef.invokeMethodAsync('OnChapterNav', 'next');
        e.preventDefault();
        return;
    }

    if (e.key === 'ArrowLeft' && e.altKey) {
        _dotNetRef.invokeMethodAsync('OnChapterNav', 'prev');
        e.preventDefault();
        return;
    }

    if (e.key === 'ArrowRight' && (e.ctrlKey || e.metaKey)) {
        _dotNetRef.invokeMethodAsync('OnCrossNav', 'errors-to-playback');
        e.preventDefault();
        return;
    }

    if (e.key === 'ArrowLeft' && (e.ctrlKey || e.metaKey)) {
        _dotNetRef.invokeMethodAsync('OnCrossNav', 'playback-to-errors');
        e.preventDefault();
        return;
    }

    if (e.key === 'ArrowLeft') {
        _dotNetRef.invokeMethodAsync('OnSwitchView', 'prev');
        e.preventDefault();
        return;
    }

    if (e.key === 'ArrowRight') {
        _dotNetRef.invokeMethodAsync('OnSwitchView', 'next');
        e.preventDefault();
        return;
    }

    if (e.key === 'ArrowUp') {
        _dotNetRef.invokeMethodAsync('OnNavigateItem', 'prev');
        e.preventDefault();
        return;
    }

    if (e.key === 'ArrowDown') {
        _dotNetRef.invokeMethodAsync('OnNavigateItem', 'next');
        e.preventDefault();
        return;
    }

    if ((e.key === 'd' || e.key === 'D') && !e.ctrlKey && !e.metaKey && !e.altKey) {
        _dotNetRef.invokeMethodAsync('OnToggleReviewed');
        e.preventDefault();
        return;
    }

    if ((e.key === 'e' || e.key === 'E') && !e.ctrlKey && !e.metaKey && !e.altKey) {
        _dotNetRef.invokeMethodAsync('OnOpenCrx');
        e.preventDefault();
        return;
    }

    if (e.key === ' ' && !e.ctrlKey && !e.metaKey && !e.altKey) {
        _dotNetRef.invokeMethodAsync('OnSpacebar');
        e.preventDefault();
        return;
    }
}

export function init(dotNetRef) {
    _dotNetRef = dotNetRef;
    _handler = handleKeydown;
    document.addEventListener('keydown', _handler);
}

export function dispose() {
    if (_handler) {
        document.removeEventListener('keydown', _handler);
        _handler = null;
    }
    _dotNetRef = null;
}

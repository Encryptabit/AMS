// Touch gesture dispatcher for ChapterReview.
// Detects long-press + horizontal swipe gestures while preserving vertical scroll behavior.
// Communicates with Blazor via DotNetObjectReference JSInvokable calls.

let _dotNetRef = null;
let _activeGesture = null;

const SETTINGS = {
    longPressDelayMs: 460,
    longPressMoveTolerancePx: 12,
    swipeMinDistancePx: 72,
    swipeMaxVerticalDriftPx: 56,
    swipeDirectionRatio: 1.25,
    swipeMaxDurationMs: 900,
};

function isModalOpen() {
    return document.querySelector('.crx-modal-overlay.visible') !== null
        || document.querySelector('.ignore-modal-overlay.visible') !== null;
}

function isInteractiveTarget(target) {
    if (!target || !(target instanceof Element)) {
        return false;
    }

    return target.closest('input, textarea, select, button, a, [role="button"], [contenteditable="true"]') !== null;
}

function parseSentenceId(idValue, prefix) {
    if (!idValue || !idValue.startsWith(prefix)) {
        return null;
    }

    const parsed = Number.parseInt(idValue.substring(prefix.length), 10);
    if (!Number.isFinite(parsed) || parsed <= 0) {
        return null;
    }

    return parsed;
}

function resolveGestureContext(target) {
    if (!target || !(target instanceof Element)) {
        return null;
    }

    const playbackRow = target.closest('[data-ams-proof-gesture-surface="playback"] [id^="sentence-"]')
        ?? target.closest('[id^="sentence-"]');
    if (playbackRow) {
        const sentenceId = parseSentenceId(playbackRow.id, 'sentence-');
        if (sentenceId !== null) {
            return {
                sentenceId,
                surface: 'playback',
            };
        }
    }

    const errorRow = target.closest('[data-ams-proof-gesture-surface="errors"] [id^="error-card-"]')
        ?? target.closest('[id^="error-card-"]');
    if (errorRow) {
        const sentenceId = parseSentenceId(errorRow.id, 'error-card-');
        if (sentenceId !== null) {
            return {
                sentenceId,
                surface: 'errors',
            };
        }
    }

    return null;
}

function getSelectionModeState() {
    const indicator = document.querySelector('[data-ams-proof-selection-mode-state]');
    if (!indicator) {
        return 'unknown';
    }

    return indicator.getAttribute('data-ams-proof-selection-mode-state') ?? 'unknown';
}

function isSelectionModeActive() {
    return getSelectionModeState() === 'active';
}

function clearLongPressTimer(gesture) {
    if (!gesture || !gesture.longPressTimer) {
        return;
    }

    clearTimeout(gesture.longPressTimer);
    gesture.longPressTimer = null;
}

function clearActiveGesture() {
    if (_activeGesture) {
        clearLongPressTimer(_activeGesture);
    }

    _activeGesture = null;
}

async function dispatchGesture(methodName, ...args) {
    if (!_dotNetRef) {
        return;
    }

    try {
        await _dotNetRef.invokeMethodAsync(methodName, ...args);
    } catch (error) {
        const message = error instanceof Error ? error.message : String(error);
        console.warn(`[ProofSelectionMode] event=gesture-dispatch-failed; method=${methodName}; details=${message}`);
    }
}

function findChangedTouch(touchList, identifier) {
    for (const touch of touchList) {
        if (touch.identifier === identifier) {
            return touch;
        }
    }

    return null;
}

function armLongPress(gesture) {
    clearLongPressTimer(gesture);

    gesture.longPressTimer = setTimeout(() => {
        if (!_activeGesture || _activeGesture.identifier !== gesture.identifier) {
            return;
        }

        if (gesture.movedTooFar) {
            return;
        }

        gesture.longPressTriggered = true;
        void dispatchGesture('OnSentenceLongPress', gesture.sentenceId);
    }, SETTINGS.longPressDelayMs);
}

function handleTouchStart(e) {
    if (!_dotNetRef || e.touches.length !== 1 || isModalOpen()) {
        return;
    }

    const touch = e.changedTouches[0];
    const context = resolveGestureContext(e.target);
    if (!context) {
        return;
    }

    if (isInteractiveTarget(e.target)) {
        return;
    }

    clearActiveGesture();

    _activeGesture = {
        identifier: touch.identifier,
        sentenceId: context.sentenceId,
        surface: context.surface,
        startX: touch.clientX,
        startY: touch.clientY,
        lastX: touch.clientX,
        lastY: touch.clientY,
        startTimeMs: performance.now(),
        movedTooFar: false,
        longPressTriggered: false,
        longPressTimer: null,
    };

    armLongPress(_activeGesture);
}

function handleTouchMove(e) {
    if (!_activeGesture) {
        return;
    }

    const touch = findChangedTouch(e.changedTouches, _activeGesture.identifier);
    if (!touch) {
        return;
    }

    _activeGesture.lastX = touch.clientX;
    _activeGesture.lastY = touch.clientY;

    const deltaX = touch.clientX - _activeGesture.startX;
    const deltaY = touch.clientY - _activeGesture.startY;
    const movedDistance = Math.hypot(deltaX, deltaY);

    if (movedDistance > SETTINGS.longPressMoveTolerancePx) {
        _activeGesture.movedTooFar = true;
        clearLongPressTimer(_activeGesture);
    }
}

function isSwipeGesture(gesture, deltaX, deltaY, durationMs) {
    if (durationMs > SETTINGS.swipeMaxDurationMs) {
        return false;
    }

    if (!isSelectionModeActive()) {
        return false;
    }

    const absX = Math.abs(deltaX);
    const absY = Math.abs(deltaY);

    if (absX < SETTINGS.swipeMinDistancePx) {
        return false;
    }

    if (absY > SETTINGS.swipeMaxVerticalDriftPx) {
        return false;
    }

    if (absX < absY * SETTINGS.swipeDirectionRatio) {
        return false;
    }

    return true;
}

function dispatchSwipe(gesture, deltaX) {
    if (deltaX > 0) {
        void dispatchGesture('OnSelectionSwipeRight', gesture.sentenceId, gesture.surface);
        return;
    }

    void dispatchGesture('OnSelectionSwipeLeft', gesture.sentenceId, gesture.surface);
}

function handleTouchEnd(e) {
    if (!_activeGesture) {
        return;
    }

    const touch = findChangedTouch(e.changedTouches, _activeGesture.identifier);
    if (!touch) {
        return;
    }

    const gesture = _activeGesture;
    clearLongPressTimer(gesture);
    _activeGesture = null;

    if (gesture.longPressTriggered) {
        return;
    }

    const deltaX = touch.clientX - gesture.startX;
    const deltaY = touch.clientY - gesture.startY;
    const durationMs = performance.now() - gesture.startTimeMs;

    if (isSwipeGesture(gesture, deltaX, deltaY, durationMs)) {
        dispatchSwipe(gesture, deltaX);
    }
}

function handleTouchCancel(e) {
    if (!_activeGesture) {
        return;
    }

    const touch = findChangedTouch(e.changedTouches, _activeGesture.identifier);
    if (!touch) {
        return;
    }

    clearActiveGesture();
}

export function init(dotNetRef) {
    _dotNetRef = dotNetRef;

    document.addEventListener('touchstart', handleTouchStart, { passive: true });
    document.addEventListener('touchmove', handleTouchMove, { passive: true });
    document.addEventListener('touchend', handleTouchEnd, { passive: true });
    document.addEventListener('touchcancel', handleTouchCancel, { passive: true });
}

export function dispose() {
    document.removeEventListener('touchstart', handleTouchStart);
    document.removeEventListener('touchmove', handleTouchMove);
    document.removeEventListener('touchend', handleTouchEnd);
    document.removeEventListener('touchcancel', handleTouchCancel);

    clearActiveGesture();
    _dotNetRef = null;
}

// Lightweight audio snippet player for inline playback from Errors view
// Uses a cached HTML5 Audio element — no waveform visualization needed.

let audio = null;
let currentUrl = null;
let stopTimer = null;

/**
 * Plays an audio segment from startTime to endTime.
 * Caches the Audio object so repeated plays for the same URL are instant.
 */
export function playSegment(url, startTime, endTime) {
    stopCurrent();

    if (currentUrl !== url || !audio) {
        if (audio) {
            audio.pause();
            audio.src = '';
        }
        audio = new Audio(url);
        currentUrl = url;
    }

    audio.currentTime = startTime;
    audio.play();

    const duration = (endTime - startTime) * 1000;
    stopTimer = setTimeout(() => {
        if (audio) audio.pause();
    }, duration);
}

/**
 * Stops any currently playing snippet.
 */
export function stopCurrent() {
    if (stopTimer) {
        clearTimeout(stopTimer);
        stopTimer = null;
    }
    if (audio) audio.pause();
}

/**
 * Releases the cached Audio element.
 */
export function dispose() {
    stopCurrent();
    if (audio) {
        audio.src = '';
        audio = null;
    }
    currentUrl = null;
}

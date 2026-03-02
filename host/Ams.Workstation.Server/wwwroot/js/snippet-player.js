// Lightweight audio snippet player for inline playback from Errors view
// Uses a cached HTML5 Audio element — no waveform visualization needed.

let audio = null;
let currentUrl = null;

/**
 * Plays an audio clip URL.
 * Caches the Audio object so repeated plays for the same URL are instant.
 */
export async function playSegment(url) {
    stopCurrent();

    if (currentUrl !== url || !audio) {
        if (audio) {
            audio.pause();
            audio.src = '';
        }
        audio = new Audio(url);
        currentUrl = url;
    }

    if (!audio) return;

    await waitForMetadata(audio);
    audio.currentTime = 0;

    try {
        await audio.play();
    } catch {
        // Ignore browser playback interruptions; caller can retry with another click.
    }
}

/**
 * Stops any currently playing snippet.
 */
export function stopCurrent() {
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

function waitForMetadata(player) {
    if (player.readyState >= 1) {
        return Promise.resolve();
    }

    return new Promise(resolve => {
        const onLoadedMetadata = () => {
            player.removeEventListener('loadedmetadata', onLoadedMetadata);
            resolve();
        };
        player.addEventListener('loadedmetadata', onLoadedMetadata, { once: true });
    });
}

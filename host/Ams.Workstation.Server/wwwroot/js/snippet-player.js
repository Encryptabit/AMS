// Lightweight audio snippet player for inline playback from Errors view
// Uses a cached HTML5 Audio element — no waveform visualization needed.

let audio = null;
let currentUrl = null;
let activeDotNetRef = null;
let activeSyncToken = 0;
let timeUpdateHandler = null;
let endedHandler = null;

/**
 * Plays an audio clip URL.
 * Caches the Audio object so repeated plays for the same URL are instant.
 */
export async function playSegment(url, dotNetRef, syncToken) {
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
    wireSidecarSync(dotNetRef, syncToken);

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
    if (audio) {
        audio.pause();
    }
    clearSidecarSyncHandlers();
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
    activeDotNetRef = null;
    activeSyncToken = 0;
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

function wireSidecarSync(dotNetRef, syncToken) {
    clearSidecarSyncHandlers();

    activeDotNetRef = dotNetRef || null;
    activeSyncToken = Number.isInteger(syncToken) ? syncToken : 0;
    if (!audio || !activeDotNetRef) return;

    timeUpdateHandler = () => {
        activeDotNetRef.invokeMethodAsync('OnSidecarTimeUpdate', audio.currentTime, activeSyncToken);
    };

    endedHandler = () => {
        activeDotNetRef.invokeMethodAsync('OnSidecarPlaybackEnded', activeSyncToken);
    };

    audio.addEventListener('timeupdate', timeUpdateHandler);
    audio.addEventListener('ended', endedHandler);
}

function clearSidecarSyncHandlers() {
    if (!audio) return;

    if (timeUpdateHandler) {
        audio.removeEventListener('timeupdate', timeUpdateHandler);
        timeUpdateHandler = null;
    }

    if (endedHandler) {
        audio.removeEventListener('ended', endedHandler);
        endedHandler = null;
    }
}

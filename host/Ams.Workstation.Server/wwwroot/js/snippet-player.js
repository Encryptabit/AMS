// Lightweight audio snippet player for inline playback from Errors view
// Uses a cached HTML5 Audio element — no waveform visualization needed.

let audio = null;
let currentUrl = null;
let activeDotNetRef = null;
let activeSyncToken = 0;
let timeUpdateHandler = null;
let endedHandler = null;
let activeWaveformElementId = null;
let activeChapterStartSec = 0;
let activeChapterEndSec = 0;
let syncRafId = null;

/**
 * Plays an audio clip URL.
 * Caches the Audio object so repeated plays for the same URL are instant.
 */
export async function playSegment(url, arg2, arg3, arg4) {
    stopCurrent();

    const { dotNetRef, syncToken, syncOptions } = resolveSyncArgs(arg2, arg3, arg4);

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
        startWaveformSync(syncOptions);
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
    stopWaveformSync();
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
        syncWaveformToEnd();
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

function resolveSyncArgs(arg2, arg3, arg4) {
    const isDotNetRef = !!arg2 && typeof arg2.invokeMethodAsync === 'function';
    if (isDotNetRef) {
        return {
            dotNetRef: arg2,
            syncToken: Number.isInteger(arg3) ? arg3 : 0,
            syncOptions: arg4 || null
        };
    }

    return {
        dotNetRef: null,
        syncToken: 0,
        syncOptions: arg2 || null
    };
}

function startWaveformSync(syncOptions) {
    stopWaveformSync();

    if (!audio || !syncOptions || !syncOptions.waveformElementId) {
        return;
    }

    activeWaveformElementId = syncOptions.waveformElementId;
    activeChapterStartSec = toFiniteNumber(syncOptions.chapterStartSec, 0);
    activeChapterEndSec = toFiniteNumber(syncOptions.chapterEndSec, activeChapterStartSec);
    if (activeChapterEndSec < activeChapterStartSec) {
        activeChapterEndSec = activeChapterStartSec;
    }

    syncWaveformNow();

    const loop = () => {
        if (!audio || audio.ended) {
            stopWaveformSync();
            return;
        }

        syncWaveformNow();

        if (!audio.paused) {
            syncRafId = requestAnimationFrame(loop);
        } else {
            syncRafId = null;
        }
    };

    syncRafId = requestAnimationFrame(loop);
}

function stopWaveformSync() {
    if (syncRafId !== null) {
        cancelAnimationFrame(syncRafId);
        syncRafId = null;
    }

    activeWaveformElementId = null;
    activeChapterStartSec = 0;
    activeChapterEndSec = 0;
}

function syncWaveformToEnd() {
    if (!activeWaveformElementId) return;
    const ws = getActiveWaveSurfer();
    if (!ws) return;

    const duration = ws.getDuration();
    if (!duration || duration <= 0) return;
    ws.seekTo(clamp(activeChapterEndSec, 0, duration) / duration);
}

function syncWaveformNow() {
    if (!audio || !activeWaveformElementId) return;

    const ws = getActiveWaveSurfer();
    if (!ws) return;

    const duration = ws.getDuration();
    if (!duration || duration <= 0) return;

    const chapterTime = clamp(
        activeChapterStartSec + Math.max(0, audio.currentTime || 0),
        activeChapterStartSec,
        activeChapterEndSec);

    ws.seekTo(chapterTime / duration);
}

function getActiveWaveSurfer() {
    if (!activeWaveformElementId) return null;
    const instance = window.wavesurferInstances && window.wavesurferInstances[activeWaveformElementId];
    return instance && instance.wavesurfer ? instance.wavesurfer : null;
}

function toFiniteNumber(value, fallback) {
    const n = Number(value);
    return Number.isFinite(n) ? n : fallback;
}

function clamp(value, min, max) {
    return Math.min(max, Math.max(min, value));
}

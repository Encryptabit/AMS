// Waveform JS Interop Module for Blazor
// Wraps wavesurfer.js v7 for use with Blazor Server components

// Global instance registry keyed by element ID
window.wavesurferInstances = window.wavesurferInstances || {};

/**
 * Creates a new WaveSurfer instance and stores it in the registry.
 * @param {string} elementId - The DOM element ID for the waveform container
 * @param {Object} options - WaveSurfer configuration options
 * @returns {string} The element ID (for chaining)
 */
export function createWaveSurfer(elementId, options) {
    const container = document.getElementById(elementId);
    if (!container) {
        console.error(`[waveform-interop] Container element '${elementId}' not found`);
        return null;
    }

    // Destroy existing instance if present
    if (window.wavesurferInstances[elementId]) {
        try {
            window.wavesurferInstances[elementId].wavesurfer.destroy();
        } catch (e) {
            console.warn(`[waveform-interop] Error destroying existing instance:`, e);
        }
        delete window.wavesurferInstances[elementId];
    }

    const wsOptions = {
        container: container,
        height: options.height || 128,
        waveColor: options.waveColor || '#4a9eff',
        progressColor: options.progressColor || '#1177bb',
        cursorColor: options.cursorColor || '#ffffff',
        normalize: true,
        backend: 'MediaElement',
        pixelRatio: window.devicePixelRatio || 1,
        minPxPerSec: options.minPxPerSec || 50,
        fillParent: true,
        scrollParent: options.scrollParent || false,
        autoCenter: options.autoCenter !== undefined ? options.autoCenter : true,
        hideScrollbar: options.hideScrollbar !== undefined ? options.hideScrollbar : true,
        barHeight: options.barHeight || 1,
    };

    // Only add bar properties if explicitly set (non-zero)
    if (options.barWidth && options.barWidth !== 0) {
        wsOptions.barWidth = options.barWidth;
    }
    if (options.barGap && options.barGap !== 0) {
        wsOptions.barGap = options.barGap;
    }
    if (options.barRadius && options.barRadius !== 0) {
        wsOptions.barRadius = options.barRadius;
    }

    const wavesurfer = WaveSurfer.create(wsOptions);

    // Store instance with metadata
    window.wavesurferInstances[elementId] = {
        wavesurfer: wavesurfer,
        regionsPlugin: null,
        dotNetRef: null,
        isPlaying: false,
        preservePitch: true
    };

    return elementId;
}

/**
 * Loads audio from a URL into the WaveSurfer instance.
 * @param {string} elementId - The element ID of the WaveSurfer instance
 * @param {string} url - The URL of the audio file to load
 */
export function loadAudio(elementId, url) {
    const instance = window.wavesurferInstances[elementId];
    if (!instance) {
        console.error(`[waveform-interop] No WaveSurfer instance for '${elementId}'`);
        return;
    }
    instance.wavesurfer.load(url);
}

/**
 * Starts playback.
 * @param {string} elementId - The element ID of the WaveSurfer instance
 */
export function play(elementId) {
    const instance = window.wavesurferInstances[elementId];
    if (!instance) return;
    instance.wavesurfer.play();
}

/**
 * Pauses playback.
 * @param {string} elementId - The element ID of the WaveSurfer instance
 */
export function pause(elementId) {
    const instance = window.wavesurferInstances[elementId];
    if (!instance) return;
    instance.wavesurfer.pause();
}

/**
 * Toggles play/pause state.
 * @param {string} elementId - The element ID of the WaveSurfer instance
 */
export function playPause(elementId) {
    const instance = window.wavesurferInstances[elementId];
    if (!instance) return;
    instance.wavesurfer.playPause();
}

/**
 * Seeks to a specific time in seconds.
 * @param {string} elementId - The element ID of the WaveSurfer instance
 * @param {number} seconds - The time in seconds to seek to
 */
export function seekTo(elementId, seconds) {
    const instance = window.wavesurferInstances[elementId];
    if (!instance) return;

    const duration = instance.wavesurfer.getDuration();
    if (duration > 0) {
        // seekTo expects a ratio 0-1
        instance.wavesurfer.seekTo(seconds / duration);
    }
}

/**
 * Seeks to the start of the audio.
 * @param {string} elementId - The element ID of the WaveSurfer instance
 */
export function seekToStart(elementId) {
    const instance = window.wavesurferInstances[elementId];
    if (!instance) return;
    instance.wavesurfer.seekTo(0);
}

/**
 * Seeks to the end of the audio.
 * @param {string} elementId - The element ID of the WaveSurfer instance
 */
export function seekToEnd(elementId) {
    const instance = window.wavesurferInstances[elementId];
    if (!instance) return;
    instance.wavesurfer.seekTo(1);
}

/**
 * Gets the current playback time in seconds.
 * @param {string} elementId - The element ID of the WaveSurfer instance
 * @returns {number} The current time in seconds
 */
export function getCurrentTime(elementId) {
    const instance = window.wavesurferInstances[elementId];
    if (!instance) return 0;
    return instance.wavesurfer.getCurrentTime();
}

/**
 * Gets the total audio duration in seconds.
 * @param {string} elementId - The element ID of the WaveSurfer instance
 * @returns {number} The duration in seconds
 */
export function getDuration(elementId) {
    const instance = window.wavesurferInstances[elementId];
    if (!instance) return 0;
    return instance.wavesurfer.getDuration();
}

/**
 * Sets the playback rate (speed).
 * @param {string} elementId - The element ID of the WaveSurfer instance
 * @param {number} rate - The playback rate (0.5 to 2.0)
 * @param {boolean} preservePitch - Whether to preserve pitch when changing speed
 */
export function setPlaybackRate(elementId, rate, preservePitch) {
    const instance = window.wavesurferInstances[elementId];
    if (!instance) return;

    instance.preservePitch = preservePitch !== undefined ? preservePitch : instance.preservePitch;
    instance.wavesurfer.setPlaybackRate(rate, instance.preservePitch);
}

/**
 * Gets the current playback rate.
 * @param {string} elementId - The element ID of the WaveSurfer instance
 * @returns {number} The current playback rate
 */
export function getPlaybackRate(elementId) {
    const instance = window.wavesurferInstances[elementId];
    if (!instance) return 1;
    return instance.wavesurfer.getPlaybackRate();
}

/**
 * Checks if audio is currently playing.
 * @param {string} elementId - The element ID of the WaveSurfer instance
 * @returns {boolean} True if playing, false otherwise
 */
export function isPlaying(elementId) {
    const instance = window.wavesurferInstances[elementId];
    if (!instance) return false;
    return instance.wavesurfer.isPlaying();
}

/**
 * Registers .NET callback methods for WaveSurfer events.
 * @param {string} elementId - The element ID of the WaveSurfer instance
 * @param {Object} dotNetRef - The DotNetObjectReference for invoking .NET methods
 */
export function registerCallbacks(elementId, dotNetRef) {
    const instance = window.wavesurferInstances[elementId];
    if (!instance) {
        console.error(`[waveform-interop] No WaveSurfer instance for '${elementId}'`);
        return;
    }

    instance.dotNetRef = dotNetRef;
    const ws = instance.wavesurfer;

    // Ready event - fires when audio is decoded and waveform is drawn
    ws.on('ready', () => {
        const duration = ws.getDuration();
        dotNetRef.invokeMethodAsync('OnWaveformReady', duration)
            .catch(err => console.warn('[waveform-interop] Error invoking OnWaveformReady:', err));
    });

    // Time update event - fires during playback and seeking
    ws.on('audioprocess', (currentTime) => {
        dotNetRef.invokeMethodAsync('OnTimeUpdate', currentTime)
            .catch(err => console.warn('[waveform-interop] Error invoking OnTimeUpdate:', err));
    });

    // Seeking event - fires when user clicks on waveform
    ws.on('seeking', (currentTime) => {
        dotNetRef.invokeMethodAsync('OnSeeking', currentTime)
            .catch(err => console.warn('[waveform-interop] Error invoking OnSeeking:', err));
    });

    // Finish event - fires when audio playback completes
    ws.on('finish', () => {
        dotNetRef.invokeMethodAsync('OnPlaybackFinished')
            .catch(err => console.warn('[waveform-interop] Error invoking OnPlaybackFinished:', err));
    });

    // Play event
    ws.on('play', () => {
        instance.isPlaying = true;
        dotNetRef.invokeMethodAsync('OnPlayStateChanged', true)
            .catch(err => console.warn('[waveform-interop] Error invoking OnPlayStateChanged:', err));
    });

    // Pause event
    ws.on('pause', () => {
        instance.isPlaying = false;
        dotNetRef.invokeMethodAsync('OnPlayStateChanged', false)
            .catch(err => console.warn('[waveform-interop] Error invoking OnPlayStateChanged:', err));
    });

    // Error event
    ws.on('error', (error) => {
        console.error('[waveform-interop] WaveSurfer error:', error);
        dotNetRef.invokeMethodAsync('OnError', error.toString())
            .catch(err => console.warn('[waveform-interop] Error invoking OnError:', err));
    });
}

/**
 * Initializes the regions plugin for the WaveSurfer instance.
 * @param {string} elementId - The element ID of the WaveSurfer instance
 */
export function initRegions(elementId) {
    const instance = window.wavesurferInstances[elementId];
    if (!instance) return;

    if (!instance.regionsPlugin) {
        // WaveSurfer.Regions is available globally from the CDN
        if (typeof WaveSurfer.Regions !== 'undefined') {
            instance.regionsPlugin = instance.wavesurfer.registerPlugin(WaveSurfer.Regions.create());
        } else {
            console.error('[waveform-interop] WaveSurfer.Regions plugin not loaded');
        }
    }
}

/**
 * Adds a highlighted region to the waveform.
 * @param {string} elementId - The element ID of the WaveSurfer instance
 * @param {string} id - Unique identifier for the region
 * @param {number} start - Start time in seconds
 * @param {number} end - End time in seconds
 * @param {string} color - CSS color string for the region (with alpha)
 */
export function addRegion(elementId, id, start, end, color) {
    const instance = window.wavesurferInstances[elementId];
    if (!instance || !instance.regionsPlugin) {
        console.warn('[waveform-interop] Regions plugin not initialized');
        return;
    }

    instance.regionsPlugin.addRegion({
        id: id,
        start: start,
        end: end,
        color: color || 'rgba(59, 130, 246, 0.3)',
        drag: false,
        resize: false
    });
}

/**
 * Removes all regions from the waveform.
 * @param {string} elementId - The element ID of the WaveSurfer instance
 */
export function clearRegions(elementId) {
    const instance = window.wavesurferInstances[elementId];
    if (!instance || !instance.regionsPlugin) return;

    instance.regionsPlugin.clearRegions();
}

/**
 * Removes a specific region by ID.
 * @param {string} elementId - The element ID of the WaveSurfer instance
 * @param {string} regionId - The ID of the region to remove
 */
export function removeRegion(elementId, regionId) {
    const instance = window.wavesurferInstances[elementId];
    if (!instance || !instance.regionsPlugin) return;

    const regions = instance.regionsPlugin.getRegions();
    const region = regions.find(r => r.id === regionId);
    if (region) {
        region.remove();
    }
}

/**
 * Highlights a specific region by clearing others with same prefix and adding/updating the target.
 * @param {string} elementId - The element ID of the WaveSurfer instance
 * @param {string} id - The region ID
 * @param {number} start - Start time in seconds
 * @param {number} end - End time in seconds
 * @param {string} color - Highlight color
 */
export function highlightRegion(elementId, id, start, end, color) {
    const instance = window.wavesurferInstances[elementId];
    if (!instance || !instance.regionsPlugin) return;

    // Clear existing regions with similar prefix (e.g., clear all 'sentence-*' regions)
    const prefix = id.split('-')[0];
    const regions = instance.regionsPlugin.getRegions();
    regions.forEach(r => {
        if (r.id.startsWith(prefix + '-')) {
            r.remove();
        }
    });

    // Add new highlight region
    instance.regionsPlugin.addRegion({
        id: id,
        start: start,
        end: end,
        color: color || 'rgba(255, 215, 0, 0.4)',
        drag: false,
        resize: false
    });
}

/**
 * Plays a specific region.
 * @param {string} elementId - The element ID of the WaveSurfer instance
 * @param {string} regionId - The ID of the region to play
 */
export function playRegion(elementId, regionId) {
    const instance = window.wavesurferInstances[elementId];
    if (!instance || !instance.regionsPlugin) return;

    const regions = instance.regionsPlugin.getRegions();
    const region = regions.find(r => r.id === regionId);
    if (region) {
        region.play();
    }
}

/**
 * Destroys the WaveSurfer instance and cleans up resources.
 * @param {string} elementId - The element ID of the WaveSurfer instance
 */
export function destroy(elementId) {
    const instance = window.wavesurferInstances[elementId];
    if (!instance) return;

    try {
        instance.wavesurfer.destroy();
    } catch (e) {
        console.warn('[waveform-interop] Error during destroy:', e);
    }

    delete window.wavesurferInstances[elementId];
}

/**
 * Formats time in seconds to mm:ss format.
 * @param {number} seconds - Time in seconds
 * @returns {string} Formatted time string
 */
export function formatTime(seconds) {
    const mins = Math.floor(seconds / 60);
    const secs = Math.floor(seconds % 60);
    return `${mins}:${secs.toString().padStart(2, '0')}`;
}

// Waveform JS Interop Module for Blazor
// Wraps wavesurfer.js v7 for use with Blazor Server components

// Global instance registry keyed by element ID
window.wavesurferInstances = window.wavesurferInstances || {};

const DEFAULT_WHEEL_ZOOM_SCALE = 0.5;
const DEFAULT_WHEEL_DELTA_THRESHOLD = 5;
const DEFAULT_MAX_ZOOM = 5000;
const DEFAULT_BAR_HEIGHT_SCALE = 0.01;

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
            destroy(elementId);
        } catch (e) {
            console.warn(`[waveform-interop] Error destroying existing instance:`, e);
        }
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
        minPxPerSec: options.minPxPerSec || 100,
        autoCenter: true,
        disableZoom: true,
        scrollParent: true,
        fillParent: false,
        hideScrollbar: false,
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
    let zoomPlugin = null;

    if (typeof WaveSurfer !== 'undefined' && typeof WaveSurfer.Zoom !== 'undefined') {
        zoomPlugin = wavesurfer.registerPlugin(WaveSurfer.Zoom.create({
            scale: options.wheelZoomScale || DEFAULT_WHEEL_ZOOM_SCALE,
            maxZoom: options.maxZoomPxPerSec || DEFAULT_MAX_ZOOM,
            deltaThreshold: options.wheelZoomDeltaThreshold || DEFAULT_WHEEL_DELTA_THRESHOLD,
        }));
    }

    // Store instance with metadata
    window.wavesurferInstances[elementId] = {
        wavesurfer: wavesurfer,
        zoomPlugin: zoomPlugin,
        regionsPlugin: null,
        dotNetRef: null,
        isPlaying: false,
        preservePitch: true,
        currentZoom: wsOptions.minPxPerSec,
        currentBarHeight: wsOptions.barHeight || 1,
        minBarHeight: options.minBarHeightScale || 0.25,
        maxBarHeight: options.maxBarHeightScale || 4,
        barHeightScale: options.barHeightWheelScale || DEFAULT_BAR_HEIGHT_SCALE,
        heightDeltaThreshold: options.heightWheelDeltaThreshold || DEFAULT_WHEEL_DELTA_THRESHOLD,
        heightAccumulatedDelta: 0,
        zoomSyncTimerId: null,
        cleanupHandlers: [],
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
    instance.lastTimeUpdate = 0;
    const ws = instance.wavesurfer;

    ws.on('zoom', (pxPerSec) => {
        instance.currentZoom = pxPerSec;
        if (instance.zoomSyncTimerId !== null) {
            clearTimeout(instance.zoomSyncTimerId);
        }

        instance.zoomSyncTimerId = setTimeout(() => {
            instance.zoomSyncTimerId = null;
            dotNetRef.invokeMethodAsync('OnZoomChanged', Math.round(pxPerSec))
                .catch(err => console.warn('[waveform-interop] Error invoking OnZoomChanged:', err));
        }, 50);
    });

    // Ready event - fires when audio is decoded and waveform is drawn
    ws.on('ready', () => {
        const duration = ws.getDuration();
        dotNetRef.invokeMethodAsync('OnWaveformReady', duration)
            .catch(err => console.warn('[waveform-interop] Error invoking OnWaveformReady:', err));
    });

    // Time update event - throttled to ~10fps to reduce Blazor re-renders
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

    attachCtrlWheelHeightHandler(instance, dotNetRef);
}

/**
 * Initializes the regions plugin for the WaveSurfer instance.
 * @param {string} elementId - The element ID of the WaveSurfer instance
 */
export function initRegions(elementId) {
    const instance = window.wavesurferInstances[elementId];
    if (!instance) return;

    ensureRegionsPlugin(instance);
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
    if (!instance) {
        console.warn('[waveform-interop] No WaveSurfer instance for addRegion');
        return;
    }

    if (!ensureRegionsPlugin(instance)) {
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
        if (instance.zoomSyncTimerId !== null) {
            clearTimeout(instance.zoomSyncTimerId);
            instance.zoomSyncTimerId = null;
        }
        if (instance.cleanupHandlers && instance.cleanupHandlers.length > 0) {
            instance.cleanupHandlers.forEach((cleanup) => {
                try {
                    cleanup();
                } catch (e) {
                    console.warn('[waveform-interop] Error during cleanup:', e);
                }
            });
        }
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

/**
 * Adds a draggable, resizable region for editing pickup boundaries.
 * Fires a .NET callback on update-end (not continuous drag events).
 * @param {string} elementId - The element ID of the WaveSurfer instance
 * @param {string} id - Unique identifier for the region
 * @param {number} start - Start time in seconds
 * @param {number} end - End time in seconds
 * @param {string} color - CSS color string for the region (with alpha)
 * @param {Object} dotNetRef - DotNetObjectReference for invoking .NET callbacks
 * @returns {Object|null} The region object, or null on failure
 */
export function addEditableRegion(elementId, id, start, end, color, dotNetRef) {
    const instance = window.wavesurferInstances[elementId];
    if (!instance) {
        console.error(`[waveform-interop] No WaveSurfer instance for '${elementId}'`);
        return null;
    }

    // Auto-initialize regions plugin if not yet initialized
    if (!ensureRegionsPlugin(instance)) {
        return null;
    }

    const region = instance.regionsPlugin.addRegion({
        id: id,
        start: start,
        end: end,
        color: color || 'rgba(59, 200, 120, 0.3)',
        drag: true,
        resize: true,
        minLength: 0.1
    });

    // Listen to update-end (NOT update) to avoid continuous drag events (Pitfall 6)
    region.on('update-end', () => {
        if (dotNetRef) {
            dotNetRef.invokeMethodAsync('OnRegionBoundsUpdated', id, region.start, region.end)
                .catch(err => console.warn('[waveform-interop] Error invoking OnRegionBoundsUpdated:', err));
        }
    });

    return region;
}

/**
 * Updates a region's boundaries programmatically from .NET side.
 * @param {string} elementId - The element ID of the WaveSurfer instance
 * @param {string} regionId - The ID of the region to update
 * @param {number} start - New start time in seconds
 * @param {number} end - New end time in seconds
 */
export function updateRegionBounds(elementId, regionId, start, end) {
    const instance = window.wavesurferInstances[elementId];
    if (!instance || !instance.regionsPlugin) return;

    const regions = instance.regionsPlugin.getRegions();
    const region = regions.find(r => r.id === regionId);
    if (region) {
        region.setOptions({ start: start, end: end });
    }
}

/**
 * Gets the current boundaries of a region.
 * @param {string} elementId - The element ID of the WaveSurfer instance
 * @param {string} regionId - The ID of the region to query
 * @returns {{ start: number, end: number } | null} The region bounds, or null if not found
 */
export function getRegionBounds(elementId, regionId) {
    const instance = window.wavesurferInstances[elementId];
    if (!instance || !instance.regionsPlugin) return null;

    const regions = instance.regionsPlugin.getRegions();
    const region = regions.find(r => r.id === regionId);
    if (region) {
        return { start: region.start, end: region.end };
    }
    return null;
}

/**
 * Synchronizes playheads across multiple WaveSurfer instances to the same time position.
 * @param {string[]} elementIds - Array of element IDs to synchronize
 * @param {number} timeSeconds - The target time in seconds
 */
export function syncPlayheads(elementIds, timeSeconds) {
    for (const elementId of elementIds) {
        const instance = window.wavesurferInstances[elementId];
        if (!instance) continue;

        const duration = instance.wavesurfer.getDuration();
        if (duration > 0) {
            instance.wavesurfer.seekTo(timeSeconds / duration);
        }
    }
}

/**
 * Plays a segment of audio from startSec to endSec, then pauses.
 * @param {string} elementId - The element ID of the WaveSurfer instance
 * @param {number} startSec - Start time in seconds
 * @param {number} endSec - End time in seconds
 */
export function playSegment(elementId, startSec, endSec) {
    const instance = window.wavesurferInstances[elementId];
    if (!instance) return;

    const ws = instance.wavesurfer;
    const duration = ws.getDuration();
    if (duration <= 0) return;

    // Seek to start position
    ws.seekTo(startSec / duration);
    ws.play();

    // Set up a listener that pauses when currentTime >= endSec
    const onAudioProcess = (currentTime) => {
        if (currentTime >= endSec) {
            ws.pause();
            ws.un('audioprocess', onAudioProcess);
        }
    };
    ws.on('audioprocess', onAudioProcess);
}

/**
 * Sets the zoom level of a WaveSurfer instance.
 * @param {string} elementId - The element ID of the WaveSurfer instance
 * @param {number} pxPerSec - Pixels per second of audio
 */
export function setZoom(elementId, pxPerSec) {
    const instance = window.wavesurferInstances[elementId];
    if (!instance) return;

    instance.currentZoom = pxPerSec;
    instance.wavesurfer.zoom(pxPerSec);
}

// --- Mini Waveform Renderer (no wavesurfer dependency) ---

/**
 * Draws a mini waveform visualization on a canvas element using amplitude data.
 * Renders centered horizontal bars - lightweight alternative to full wavesurfer instances.
 * @param {string} canvasId - The DOM element ID of the canvas
 * @param {number[]} amplitudeData - Array of normalized amplitude values (0.0 to 1.0)
 * @param {string} [color] - CSS fill color for the bars (defaults to '#4a9eff')
 */
export function drawMiniWaveform(canvasId, amplitudeData, color) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    const w = canvas.width;
    const h = canvas.height;
    ctx.clearRect(0, 0, w, h);
    ctx.fillStyle = color || '#4a9eff';
    const barW = w / amplitudeData.length;
    for (let i = 0; i < amplitudeData.length; i++) {
        const barH = amplitudeData[i] * h;
        ctx.fillRect(i * barW, (h - barH) / 2, Math.max(1, barW - 1), barH);
    }
}

/**
 * Fetches waveform amplitude data from the server and renders it on a canvas.
 * Combines the fetch from /api/audio/waveform-data with drawMiniWaveform in one call.
 * @param {string} canvasId - The DOM element ID of the canvas
 * @param {string} audioPath - Absolute path to the audio file on the server
 * @param {number|null} startSec - Optional start time in seconds
 * @param {number|null} endSec - Optional end time in seconds
 * @param {string} [color] - CSS fill color for the bars
 * @param {number} [points] - Number of amplitude data points (defaults to 100)
 */
export async function loadAndDrawMiniWaveform(canvasId, audioPath, startSec, endSec, color, points) {
    const params = new URLSearchParams({ path: audioPath, points: points || 100 });
    if (startSec != null) params.set('start', startSec);
    if (endSec != null) params.set('end', endSec);
    const response = await fetch(`/api/audio/waveform-data?${params}`);
    if (!response.ok) return;
    const data = await response.json();
    drawMiniWaveform(canvasId, data, color);
}

function ensureRegionsPlugin(instance) {
    if (!instance) return false;
    if (instance.regionsPlugin) return true;

    // WaveSurfer.Regions is available globally from the CDN.
    if (typeof WaveSurfer !== 'undefined' &&
        typeof WaveSurfer.Regions !== 'undefined') {
        instance.regionsPlugin = instance.wavesurfer.registerPlugin(WaveSurfer.Regions.create());
        return true;
    }

    console.error('[waveform-interop] WaveSurfer.Regions plugin not loaded');
    return false;
}

function attachCtrlWheelHeightHandler(instance, dotNetRef) {
    if (!instance || !instance.wavesurfer) return;

    const wheelTarget = instance.wavesurfer.getWrapper()?.parentElement;
    if (!wheelTarget) return;

    const onWheel = (event) => {
        if (!event.ctrlKey) {
            if (!event.shiftKey) {
                return;
            }

            // Preserve the expected browser gesture: Shift + wheel pans horizontally.
            // Stop the zoom plugin from consuming the event, then apply horizontal scroll.
            event.preventDefault();
            event.stopImmediatePropagation();

            const delta = Math.abs(event.deltaX) > 0 ? event.deltaX : event.deltaY;
            wheelTarget.scrollLeft += delta;
            return;
        }

        if (Math.abs(event.deltaX) >= Math.abs(event.deltaY)) {
            return;
        }

        event.preventDefault();
        event.stopPropagation();

        instance.heightAccumulatedDelta += -event.deltaY;
        if (instance.heightDeltaThreshold !== 0 &&
            Math.abs(instance.heightAccumulatedDelta) < instance.heightDeltaThreshold) {
            return;
        }

        const nextBarHeight = clampNumber(
            Number((instance.currentBarHeight + instance.heightAccumulatedDelta * instance.barHeightScale).toFixed(2)),
            instance.minBarHeight,
            instance.maxBarHeight);

        instance.heightAccumulatedDelta = 0;

        if (nextBarHeight === instance.currentBarHeight) {
            return;
        }

        instance.currentBarHeight = nextBarHeight;
        instance.wavesurfer.setOptions({ barHeight: nextBarHeight });
    };

    wheelTarget.addEventListener('wheel', onWheel, { passive: false, capture: true });
    instance.cleanupHandlers.push(() => wheelTarget.removeEventListener('wheel', onWheel, true));
}

function clampNumber(value, min, max) {
    return Math.min(max, Math.max(min, value));
}

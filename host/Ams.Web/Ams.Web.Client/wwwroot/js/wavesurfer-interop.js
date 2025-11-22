const players = new Map();

function ensure(id) {
    const p = players.get(id);
    if (!p) throw new Error(`WaveSurfer player ${id} not found`);
    return p;
}

export async function createPlayer(id, containerSelector, audioUrl, options, dotNetRef) {
    const container = document.querySelector(containerSelector);
    if (!container) throw new Error(`Container ${containerSelector} not found`);

    if (players.has(id)) {
        dispose(id);
    }

    const config = {
        container,
        backend: 'MediaElement',
        normalize: true,
        cursorColor: options.cursorColor || '#fff',
        waveColor: options.waveColor || '#4a9eff',
        progressColor: options.progressColor || '#1177bb',
        height: options.height || 120,
        barWidth: options.barWidth || 0,
        barGap: options.barGap || 0,
        barRadius: options.barRadius || 0,
        minPxPerSec: options.disableZoom ? 50 : 100,
        autoCenter: options.autoCenter ?? !options.disableZoom,
        scrollParent: options.scrollParent ?? false,
        hideScrollbar: true,
        pixelRatio: window.devicePixelRatio || 1
    };

    const ws = WaveSurfer.create(config);

    if (!options.disableZoom && WaveSurfer.Zoom) {
        ws.registerPlugin(WaveSurfer.Zoom.create({
            scale: 0.5,
            maxZoom: 5000
        }));
    }

    const state = { ws, dotNetRef };
    players.set(id, state);

    ws.on('ready', () => {
        dotNetRef?.invokeMethodAsync('OnReady', ws.getDuration());
    });

    ws.on('audioprocess', () => {
        dotNetRef?.invokeMethodAsync('OnTime', ws.getCurrentTime());
    });

    ws.on('finish', () => {
        dotNetRef?.invokeMethodAsync('OnEnded');
    });

    await ws.load(audioUrl);
    return true;
}

export function dispose(id) {
    const state = players.get(id);
    if (!state) return;
    state.ws.destroy();
    players.delete(id);
}

export function playPause(id) { ensure(id).ws.playPause(); }
export function play(id) { ensure(id).ws.play(); }
export function pause(id) { ensure(id).ws.pause(); }
export function seekTo(id, time) {
    const s = ensure(id);
    const duration = s.ws.getDuration();
    if (duration > 0) s.ws.seekTo(time / duration);
}
export function setRate(id, rate, preservePitch) {
    const s = ensure(id);
    s.ws.setPlaybackRate(rate, preservePitch ?? true);
}
export async function setAudio(id, audioUrl) {
    const s = ensure(id);
    await s.ws.load(audioUrl);
}

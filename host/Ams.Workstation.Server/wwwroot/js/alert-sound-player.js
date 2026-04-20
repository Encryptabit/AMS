// Lightweight player for playback error alerts in Proof > Playback view.
// Keeps a cached HTML5 Audio element so repeated alerts are low-latency.

let alertAudio = null;
let currentUrl = null;

export async function play(url) {
    if (!url) return;

    if (currentUrl !== url || !alertAudio) {
        if (alertAudio) {
            alertAudio.pause();
            alertAudio.src = '';
        }

        alertAudio = new Audio(url);
        alertAudio.preload = 'auto';
        currentUrl = url;
    }

    if (!alertAudio) return;

    try {
        alertAudio.currentTime = 0;
        await alertAudio.play();
    } catch {
        // Ignore browser playback interruptions / autoplay policy races.
    }
}

export function dispose() {
    if (alertAudio) {
        alertAudio.pause();
        alertAudio.src = '';
        alertAudio = null;
    }

    currentUrl = null;
}

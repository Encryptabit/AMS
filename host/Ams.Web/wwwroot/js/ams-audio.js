window.amsAudio = (function () {
    function seekAndPlay(audioElement, start, end) {
        if (!audioElement) {
            return;
        }

        const startTime = typeof start === "number" ? start : 0;
        const endTime = typeof end === "number" ? end : null;

        const stopHandler = () => {
            if (endTime === null) {
                return;
            }

            if (audioElement.currentTime >= endTime) {
                audioElement.pause();
                audioElement.removeEventListener("timeupdate", stopHandler);
            }
        };

        audioElement.pause();
        audioElement.currentTime = startTime;
        audioElement.removeEventListener("timeupdate", stopHandler);
        if (endTime !== null) {
            audioElement.addEventListener("timeupdate", stopHandler);
        }
        audioElement.play().catch(() => {
            audioElement.removeEventListener("timeupdate", stopHandler);
        });
    }

    function pause(audioElement) {
        if (!audioElement) {
            return;
        }
        audioElement.pause();
    }

    function stop(audioElement) {
        if (!audioElement) {
            return;
        }
        audioElement.pause();
        audioElement.currentTime = 0;
    }

    function getCurrentTime(audioElement) {
        if (!audioElement) {
            return 0;
        }
        return audioElement.currentTime;
    }

    function setPlaybackRate(audioElement, rate) {
        if (!audioElement) {
            return;
        }
        audioElement.playbackRate = rate;
    }

    return {
        seekAndPlay,
        pause,
        stop,
        getCurrentTime,
        setPlaybackRate
    };
})();

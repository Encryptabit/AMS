// Global state
let chapters = [];
let currentChapter = null;
let currentReport = null;
let pendingCrxData = null;
let reviewedStatus = {};
let currentView = 'errors'; // 'errors' or 'playback'
let chapterAudio = null;
let currentPlayingSentence = null;
let waveSurferInstances = {}; // Track WaveSurfer instances by ID
let currentAudioSource = 'raw'; // 'raw', 'treated', or 'filtered'
let selectedSentences = []; // Track ctrl+clicked sentences for multi-select
let savedScrollPosition = 0; // Save scroll position when switching views

// WaveSurfer Audio Player Component
class WaveSurferPlayer {
    constructor(containerId, audioUrl, options = {}) {
        this.containerId = containerId;
        this.audioUrl = audioUrl;
        this.audioRate = 1;
        this.preservePitch = true;  // Track pitch preservation as instance property
        this.options = {
            title: options.title || 'Audio',
            height: options.height || 150,  // Taller for more detail
            waveColor: options.waveColor || '#4a9eff',
            progressColor: options.progressColor || '#1177bb',
            cursorColor: options.cursorColor || '#ffffff',
            barWidth: options.barWidth || 0,
            barGap: options.barGap || 0,
            barRadius: options.barRadius || 0,
            onReady: options.onReady || null,
            onTimeUpdate: options.onTimeUpdate || null,
            disableZoom: options.disableZoom || false,
            autoCenter: options.autoCenter !== undefined ? options.autoCenter : !options.disableZoom,
            scrollParent: options.scrollParent !== undefined ? options.scrollParent : false,
        };
        this.wavesurfer = null;
        this.isPlaying = false;
    }

    render() {
        const container = document.getElementById(this.containerId);
        if (!container) {
            console.error(`Container ${this.containerId} not found`);
            return;
        }

        container.innerHTML = `
            <div class="waveform-container">
                <div class="waveform-header">
                    <div class="waveform-title">${this.options.title}</div>
                    <div class="waveform-time">
                        <span id="${this.containerId}-current">0:00</span> /
                        <span id="${this.containerId}-duration">0:00</span>
                    </div>
                </div>
                <div id="${this.containerId}-waveform" class="waveform-canvas"></div>
                <div class="waveform-controls">
                    <button id="${this.containerId}-skip-start" class="waveform-control-btn" title="Skip to start">
                        <i class="fas fa-step-backward"></i>
                    </button>
                    <button id="${this.containerId}-play" class="waveform-control-btn" title="Play/Pause">
                        <i class="fas fa-play"></i>
                    </button>
                    <button id="${this.containerId}-skip-end" class="waveform-control-btn" title="Skip to end">
                        <i class="fas fa-step-forward"></i>
                    </button>
                    <div class="waveform-speed-section">
                        <button id="${this.containerId}-preserve-pitch" class="waveform-pitch-toggle active" title="Preserve pitch">
                            <i class="fas fa-clock"></i>
                        </button>
                        <input type="range" id="${this.containerId}-speed-slider"
                               min="0.25" max="4" step="0.1" value="1"
                               class="waveform-speed-slider" />
                        <span class="waveform-speed-value" id="${this.containerId}-speed">1.00x</span>
                    </div>
                </div>
            </div>
        `;

        this.init();
    }

    async init() {
        const waveformDiv = document.getElementById(`${this.containerId}-waveform`);
        if (!waveformDiv) return;

        // ZoomPlugin is available globally as WaveSurfer.Zoom
        const ZoomPlugin = WaveSurfer.Zoom;

        const wavesurferConfig = {
            container: waveformDiv,
            audioRate: this.audioRate,
            height: this.options.height,
            waveColor: this.options.waveColor,
            progressColor: this.options.progressColor,
            cursorColor: this.options.cursorColor,
            normalize: true,
            backend: 'MediaElement',
            pixelRatio: window.devicePixelRatio || 1,
            minPxPerSec: this.options.disableZoom ? 50 : 100,
            fillParent: false,
            scrollParent: this.options.scrollParent,
            autoCenter: this.options.autoCenter,
            hideScrollbar: true,
            barHeight: 1,
        };

        // Only add bar properties if they are explicitly set (non-zero)
        if (this.options.barWidth !== undefined && this.options.barWidth !== 0) {
            wavesurferConfig.barWidth = this.options.barWidth;
        }
        if (this.options.barGap !== undefined && this.options.barGap !== 0) {
            wavesurferConfig.barGap = this.options.barGap;
        }
        if (this.options.barRadius !== undefined && this.options.barRadius !== 0) {
            wavesurferConfig.barRadius = this.options.barRadius;
        }

        this.wavesurfer = WaveSurfer.create(wavesurferConfig);

        // Register Zoom plugin only if not disabled
        if (!this.options.disableZoom) {
            this.wavesurfer.registerPlugin(
                ZoomPlugin.create({
                    scale: 0.5,  // 30% magnification per scroll for finer control
                    maxZoom: 5000,  // Very high maximum zoom for extreme detail
                })
            );
        }

        // Load audio
        await this.wavesurfer.load(this.audioUrl);

        // Event listeners
        this.wavesurfer.on('ready', () => {
            const duration = this.wavesurfer.getDuration();
            document.getElementById(`${this.containerId}-duration`).textContent = this.formatTime(duration);
            if (this.options.onReady) {
                this.options.onReady(this.wavesurfer);
            }
        });

        this.wavesurfer.on('audioprocess', () => {
            const current = this.wavesurfer.getCurrentTime();
            const currentElem = document.getElementById(`${this.containerId}-current`);
            if (currentElem) {
                currentElem.textContent = this.formatTime(current);
            }
            if (this.options.onTimeUpdate) {
                this.options.onTimeUpdate(current);
            }
        });

        this.wavesurfer.on('play', () => {
            this.isPlaying = true;
            const playBtn = document.getElementById(`${this.containerId}-play`);
            if (playBtn) {
                playBtn.innerHTML = '<i class="fas fa-pause"></i>';
            }
        });

        this.wavesurfer.on('pause', () => {
            this.isPlaying = false;
            const playBtn = document.getElementById(`${this.containerId}-play`);
            if (playBtn) {
                playBtn.innerHTML = '<i class="fas fa-play"></i>';
            }
        });

        // Transport controls
        const playButton = document.getElementById(`${this.containerId}-play`);
        const skipStartButton = document.getElementById(`${this.containerId}-skip-start`);
        const skipEndButton = document.getElementById(`${this.containerId}-skip-end`);

        if (playButton) {
            playButton.onclick = () => this.playPause();
        }
        if (skipStartButton) {
            skipStartButton.onclick = () => {
                this.wavesurfer.seekTo(0);
            };
        }
        if (skipEndButton) {
            skipEndButton.onclick = () => {
                this.wavesurfer.seekTo(1);
            };
        }

        // Speed controls
        const speedSlider = document.getElementById(`${this.containerId}-speed-slider`);
        const speedDisplay = document.getElementById(`${this.containerId}-speed`);
        const pitchToggle = document.getElementById(`${this.containerId}-preserve-pitch`);

        // Initialize preservePitch state
        this.preservePitch = true;

        if (speedSlider && speedDisplay) {
            speedSlider.addEventListener('input', (e) => {
                const speed = parseFloat(e.target.value);
                speedDisplay.textContent = speed.toFixed(2) + 'x';
                this.wavesurfer.setPlaybackRate(speed, this.preservePitch);
            });
        }

        if (pitchToggle) {
            pitchToggle.addEventListener('click', (e) => {
                this.preservePitch = !this.preservePitch;
                pitchToggle.classList.toggle('active', this.preservePitch);
                this.wavesurfer.setPlaybackRate(this.wavesurfer.getPlaybackRate(), this.preservePitch);
            });
        }

        // Store instance
        waveSurferInstances[this.containerId] = this;
    }

    playPause() {
        if (this.wavesurfer) {
            this.wavesurfer.playPause();
        }
    }

    play() {
        if (this.wavesurfer) {
            this.wavesurfer.play();
        }
    }

    pause() {
        if (this.wavesurfer) {
            this.wavesurfer.pause();
        }
    }

    seekTo(time) {
        if (this.wavesurfer) {
            const duration = this.wavesurfer.getDuration();
            if (duration > 0) {
                this.wavesurfer.seekTo(time / duration);
            }
        }
    }

    destroy() {
        if (this.wavesurfer) {
            this.wavesurfer.destroy();
            delete waveSurferInstances[this.containerId];
        }
    }

    formatTime(seconds) {
        const mins = Math.floor(seconds / 60);
        const secs = Math.floor(seconds % 60);
        return `${mins}:${secs.toString().padStart(2, '0')}`;
    }
}

// Initialization
async function loadReviewedStatus() {
    try {
        const response = await fetch('/api/reviewed');
        reviewedStatus = await response.json();
    } catch (error) {
        console.error('Failed to load reviewed status:', error);
        reviewedStatus = {};
    }
}

async function loadChapters() {
    try {
        const response = await fetch('/api/chapters');
        chapters = await response.json();
        await loadReviewedStatus();
        renderChapterList();
    } catch (error) {
        console.error('Failed to load chapters:', error);
    }
}

// Review management
async function markReviewed(chapterName, reviewed = true) {
    try {
        const response = await fetch(`/api/reviewed/${encodeURIComponent(chapterName)}`, {
            method: 'POST',
            headers: {'Content-Type': 'application/json'},
            body: JSON.stringify({ reviewed: reviewed })
        });

        const result = await response.json();

        if (result.success) {
            reviewedStatus[chapterName] = { reviewed: reviewed, timestamp: new Date().toISOString() };
            showToast(reviewed ? `Marked "${chapterName}" as reviewed` : `Unmarked "${chapterName}"`, 'success');
            renderChapterList();
            if (currentChapter === chapterName) {
                loadReport(chapterName);
            } else if (!currentChapter) {
                loadOverview();
            }
            updateFloatingButton();
        } else {
            showToast(`Failed to update reviewed status: ${result.error}`, 'error');
        }
    } catch (err) {
        console.error('Failed to mark reviewed:', err);
        showToast('Failed to update reviewed status', 'error');
    }
}

async function resetAllReviews() {
    if (!confirm('Are you sure you want to reset review status for ALL chapters? This cannot be undone.')) {
        return;
    }

    try {
        const response = await fetch('/api/reset-reviews', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        });

        const result = await response.json();

        if (result.success) {
            reviewedStatus = {};
            showToast('Reset all review status', 'success');
            renderChapterList();
            loadOverview();
        } else {
            showToast(`Failed to reset reviews: ${result.error}`, 'error');
        }
    } catch (err) {
        console.error('Failed to reset reviews:', err);
        showToast('Failed to reset review status', 'error');
    }
}


function toggleAudioSourceMenu() {
    // Remove existing menu if any
    const existingMenu = document.getElementById('audio-source-menu');
    if (existingMenu) {
        existingMenu.remove();
        return;
    }

    const menu = document.createElement('div');
    menu.id = 'audio-source-menu';
    menu.className = 'audio-source-menu';

    const sources = [
        { id: 'raw', label: 'Raw' },
        { id: 'treated', label: 'Treated' },
        { id: 'filtered', label: 'Filtered' }
    ];

    sources.forEach(source => {
        const option = document.createElement('div');
        option.className = `audio-source-option ${currentAudioSource === source.id ? 'active' : ''}`;
        option.innerHTML = source.label;
        option.onclick = () => switchAudioSource(source.id);
        menu.appendChild(option);
    });

    // Position menu above the button
    const button = document.getElementById('audio-source-button');
    const rect = button.getBoundingClientRect();
    menu.style.bottom = `${window.innerHeight - rect.top + 8}px`;
    menu.style.right = `${window.innerWidth - rect.right}px`;

    document.body.appendChild(menu);

    // Close menu when clicking outside
    setTimeout(() => {
        document.addEventListener('click', function closeMenu(e) {
            if (!menu.contains(e.target) && e.target.id !== 'audio-source-button') {
                menu.remove();
                document.removeEventListener('click', closeMenu);
            }
        });
    }, 0);
}

function switchAudioSource(source) {
    currentAudioSource = source;

    // Close menu
    const menu = document.getElementById('audio-source-menu');
    if (menu) menu.remove();

    // Update button text
    updateFloatingButton();

    // Reload audio for playback view
    if (currentView === 'playback' && currentReport) {
        setupChapterAudio();
    }

    // Note: Audio segments in errors view will automatically use the new source
    // on next play since they call playAudioSegment which uses currentAudioSource
}

function getAudioUrlForSource(chapterName, source) {
    // Base URL for the chapter
    let url = `/api/audio/${encodeURIComponent(chapterName)}`;

    // Add source parameter
    if (source !== 'raw') {
        url += `?source=${source}`;
    }

    return url;
}

// Chapter list rendering
function renderChapterList() {
    const list = document.getElementById('chapter-list');

    let html = `
        <div class="chapter-item overview-item" onclick="loadOverview()">
            <div class="chapter-name">📊 Book Overview</div>
            <div class="chapter-stats">All Chapters</div>
        </div>
    `;

    html += chapters.map(chapter => {
        const m = chapter.metrics;
        const isReviewed = reviewedStatus[chapter.name]?.reviewed;
        const reviewedClass = isReviewed ? 'reviewed' : '';
        const flaggedInfo = `${m.sentenceFlagged} sentences, ${m.paragraphFlagged} paragraphs`;
        return `
            <div class="chapter-item ${reviewedClass}" onclick="loadReport('${chapter.name}')">
                <div class="chapter-name">${chapter.name}</div>
                <div class="chapter-stats">Flagged: ${flaggedInfo}</div>
                <div class="chapter-stats">Avg WER: ${m.sentenceAvgWer}</div>
            </div>
        `;
    }).join('');

    list.innerHTML = html;
}

// Overview rendering
async function loadOverview() {
    currentChapter = null;

    document.querySelectorAll('.chapter-item').forEach(item => {
        if (item.classList.contains('overview-item')) {
            item.classList.add('active');
        } else {
            item.classList.remove('active');
        }
    });

    try {
        const response = await fetch('/api/overview');
        const overview = await response.json();
        renderOverview(overview);
        updateFloatingButton();
        const contentDiv = document.getElementById('content');
        if (contentDiv) {
            contentDiv.scrollTo({ top: 0, behavior: 'smooth' });
        }
    } catch (error) {
        console.error('Failed to load overview:', error);
        document.getElementById('content').innerHTML = '<div id="loading">Failed to load overview</div>';
    }
}

function renderOverview(overview) {
    const content = document.getElementById('content');

    const chaptersHtml = overview.chapters.map(c => {
        const m = c.metrics;
        const werClass = parseFloat(m.sentenceAvgWer) > 5 ? 'high' : parseFloat(m.sentenceAvgWer) > 2 ? 'medium' : 'low';
        const isReviewed = reviewedStatus[c.name]?.reviewed;
        const reviewedClass = isReviewed ? 'reviewed' : '';

        return `
            <div class="chapter-card ${reviewedClass}" onclick="loadReport('${c.name}')">
                <div class="chapter-card-header">
                    <div class="chapter-card-name">${c.name}</div>
                </div>
                <div class="chapter-card-metrics">
                    <div class="metric">
                        <span class="metric-label">Sentences:</span>
                        <span class="metric-value">${m.sentenceCount} (${m.sentenceFlagged} flagged)</span>
                    </div>
                    <div class="metric">
                        <span class="metric-label">Paragraphs:</span>
                        <span class="metric-value">${m.paragraphCount} (${m.paragraphFlagged} flagged)</span>
                    </div>
                    <div class="metric">
                        <span class="metric-label">Avg WER:</span>
                        <span class="metric-value ${werClass}">${m.sentenceAvgWer}</span>
                    </div>
                </div>
            </div>
        `;
    }).join('');

    content.innerHTML = `
        <div class="report-header">
            <h1>${overview.bookName} - Validation Overview</h1>
        </div>

        <div class="stats-grid">
            <div class="stat-card">
                <div class="stat-label">Total Chapters</div>
                <div class="stat-value">${overview.chapterCount}</div>
            </div>
            <div class="stat-card">
                <div class="stat-label">Total Sentences</div>
                <div class="stat-value">${overview.totalSentences}</div>
                <div class="stat-detail">Flagged: ${overview.totalFlaggedSentences}</div>
                <div class="stat-detail">Avg WER: ${overview.avgSentenceWer}</div>
            </div>
            <div class="stat-card">
                <div class="stat-label">Total Paragraphs</div>
                <div class="stat-value">${overview.totalParagraphs}</div>
                <div class="stat-detail">Flagged: ${overview.totalFlaggedParagraphs}</div>
                <div class="stat-detail">Avg WER: ${overview.avgParagraphWer}</div>
            </div>
        </div>

        <div class="section-title" style="display: flex; justify-content: space-between; align-items: center;">
            <span>Chapters</span>
            <button class="reset-review-button" onclick="resetAllReviews()">Reset All Review Status</button>
        </div>
        <div class="chapter-grid">
            ${chaptersHtml}
        </div>
    `;
}

// Report loading and rendering
async function loadReport(chapterName) {
    currentChapter = chapterName;

    document.querySelectorAll('.chapter-item').forEach((item) => {
        if (item.classList.contains('overview-item')) {
            item.classList.remove('active');
        } else {
            const itemName = item.querySelector('.chapter-name').textContent;
            if (itemName === chapterName) {
                item.classList.add('active');
            } else {
                item.classList.remove('active');
            }
        }
    });

    try {
        const response = await fetch(`/api/report/${encodeURIComponent(chapterName)}`);
        const report = await response.json();
        renderReport(report);
        updateFloatingButton();
        const contentDiv = document.getElementById('content');
        if (contentDiv) {
            contentDiv.scrollTo({ top: 0, behavior: 'smooth' });
        }
    } catch (error) {
        console.error('Failed to load report:', error);
        document.getElementById('content').innerHTML = '<div id="loading">Failed to load report</div>';
    }
}

// View management
function switchView(view) {
    // Save scroll position if leaving errors view
    const contentDiv = document.getElementById('content');
    if (currentView === 'errors' && contentDiv) {
        savedScrollPosition = contentDiv.scrollTop;
    }

    currentView = view;
    document.querySelectorAll('.view-toggle-button').forEach(btn => btn.classList.remove('active'));
    document.querySelector(`[data-view="${view}"]`).classList.add('active');

    // Clear multi-select when switching views
    clearSentenceSelection();

    // Update floating action bar buttons
    updateFloatingButton();

    if (currentReport) {
        renderReport(currentReport);

        // Restore scroll position if returning to errors view
        if (view === 'errors' && contentDiv && savedScrollPosition > 0) {
            setTimeout(() => {
                contentDiv.scrollTop = savedScrollPosition;
            }, 50);
        }
    }
}

function hasErrors(sentence) {
    if (!sentence.diff || !sentence.diff.ops) return false;
    return sentence.diff.ops.some(op => op.op === 'delete' || op.op === 'insert');
}

// Playback view
function renderPlaybackView(report) {
    const content = document.getElementById('content');

    // Sentences should be in chronological order for playback
    // (already sorted by ID from backend)
    const compactSentencesHtml = report.sentences.map(s => {
        const hasErrorsClass = hasErrors(s) ? (s.status === 'unreliable' ? 'has-errors unreliable' : 'has-errors') : '';
        return `
            <div id="compact-sentence-${s.id}"
                 class="compact-sentence ${hasErrorsClass}"
                 data-sentence-id="${s.id}"
                 data-start="${s.startTime}"
                 data-end="${s.endTime}">
                <div class="compact-header">
                    <span class="compact-id">#${s.id}</span>
                    <span class="compact-timing">${formatTime(s.startTime)} - ${formatTime(s.endTime)}</span>
                </div>
                <div class="compact-text">${escapeHtml(s.bookText)}</div>
            </div>
        `;
    }).join('');

    content.innerHTML = `
        <div class="report-header">
            <h1>${report.chapterName}</h1>
        </div>

        <div class="view-toggle">
            <button class="view-toggle-button" data-view="errors" onclick="switchView('errors')">Errors View</button>
            <button class="view-toggle-button active" data-view="playback" onclick="switchView('playback')">Playback View</button>
        </div>

        <div id="chapter-audio-player"></div>

        <div class="section-title">
            Chapter Sentences
            <span class="mobile-only">(Tap to seek • Swipe ← to select • Swipe → to export)</span>
            <span class="desktop-only">(Click to seek • Shift+Click or E for CRX)</span>
        </div>
        ${compactSentencesHtml}
    `;

    setupChapterAudio();
    setupCompactSentenceListeners();
}

function setupCompactSentenceListeners() {
    const sentences = document.querySelectorAll('.compact-sentence');
    sentences.forEach(sentence => {
        const sentenceId = parseInt(sentence.dataset.sentenceId);
        const startTime = parseFloat(sentence.dataset.start);

        // Track hover for 'e' key functionality
        sentence.addEventListener('mouseenter', () => {
            window.lastHoveredSentence = sentenceId;
        });

        // Touch gesture support for mobile
        let touchStartX = 0;
        let touchStartY = 0;
        let touchStartTime = 0;

        sentence.addEventListener('touchstart', (e) => {
            touchStartX = e.touches[0].clientX;
            touchStartY = e.touches[0].clientY;
            touchStartTime = Date.now();
        }, { passive: true });

        sentence.addEventListener('touchend', (e) => {
            const touchEndX = e.changedTouches[0].clientX;
            const touchEndY = e.changedTouches[0].clientY;
            const touchDuration = Date.now() - touchStartTime;

            const deltaX = touchEndX - touchStartX;
            const deltaY = touchEndY - touchStartY;
            const absDeltaX = Math.abs(deltaX);
            const absDeltaY = Math.abs(deltaY);

            // Detect horizontal swipe (must be more horizontal than vertical)
            if (absDeltaX > 50 && absDeltaX > absDeltaY && touchDuration < 500) {
                e.preventDefault();

                if (deltaX < 0) {
                    // Swipe left: toggle multi-select
                    toggleSentenceSelection(sentenceId);
                    showToast(`${selectedSentences.length} sentence(s) selected`, 'success', 1500);
                } else {
                    // Swipe right: export selected or current sentence
                    if (selectedSentences.length > 0) {
                        exportSelectedSentences();
                    } else {
                        openCrxForSentence(sentenceId);
                    }
                }
                return;
            }

            // Regular tap (no significant swipe)
            if (absDeltaX < 10 && absDeltaY < 10 && touchDuration < 300) {
                seekToSentence(sentenceId, startTime);
            }
        }, { passive: false });

        // Click handler: Ctrl+click = multi-select, Shift+click = CRX export, regular click = seek
        sentence.addEventListener('click', (e) => {
            if (e.ctrlKey || e.metaKey) {
                e.preventDefault();
                toggleSentenceSelection(sentenceId);
            } else if (e.shiftKey) {
                e.preventDefault();
                openCrxForSentence(sentenceId);
            } else {
                seekToSentence(sentenceId, startTime);
            }
        });

        // Double-click handler: CRX export (backwards compatibility)
        sentence.addEventListener('dblclick', (e) => {
            e.preventDefault();
            openCrxForSentence(sentenceId);
        });
    });
}

function formatTime(seconds) {
    const mins = Math.floor(seconds / 60);
    const secs = Math.floor(seconds % 60);
    return `${mins}:${secs.toString().padStart(2, '0')}`;
}

function setupChapterAudio() {
    // Destroy existing WaveSurfer instance if any
    if (waveSurferInstances['chapter-audio-player']) {
        waveSurferInstances['chapter-audio-player'].destroy();
    }

    // Create new WaveSurfer player for chapter audio with selected source
    const audioUrl = getAudioUrlForSource(currentChapter, currentAudioSource);
    const sourceLabel = currentAudioSource.charAt(0).toUpperCase() + currentAudioSource.slice(1);
    const isMobile = window.innerWidth <= 768;
    const player = new WaveSurferPlayer('chapter-audio-player', audioUrl, {
        title: `${currentChapter} - ${sourceLabel} Audio`,
        height: isMobile ? 25 : 100,
        waveColor: '#4a9eff',
        progressColor: '#1177bb',
        disableZoom: true,
        autoCenter: true,
        scrollParent: true,
        onTimeUpdate: (currentTime) => {
            updatePlayingSentence(currentTime);
        }
    });
    player.render();

    // Store reference for seeking
    chapterAudio = player;
}

function updatePlayingSentence(currentTime) {
    if (!currentReport) return;

    const sentence = currentReport.sentences.find(s =>
        s.startTime <= currentTime && currentTime <= s.endTime
    );

    if (sentence && currentPlayingSentence !== sentence.id) {
        if (currentPlayingSentence) {
            const prev = document.getElementById(`compact-sentence-${currentPlayingSentence}`);
            if (prev) prev.classList.remove('playing');
        }

        currentPlayingSentence = sentence.id;
        const current = document.getElementById(`compact-sentence-${sentence.id}`);
        if (current) {
            current.classList.add('playing');
            current.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
    } else if (!sentence && currentPlayingSentence) {
        const prev = document.getElementById(`compact-sentence-${currentPlayingSentence}`);
        if (prev) prev.classList.remove('playing');
        currentPlayingSentence = null;
    }
}

function seekToSentence(sentenceId, startTime) {
    if (!chapterAudio) return;

    // Check if audio is ready, if not wait for it
    if (chapterAudio.wavesurfer && chapterAudio.wavesurfer.getDuration() > 0) {
        chapterAudio.seekTo(startTime);
        chapterAudio.play();
    } else {
        // Wait for audio to load
        const checkReady = setInterval(() => {
            if (chapterAudio.wavesurfer && chapterAudio.wavesurfer.getDuration() > 0) {
                clearInterval(checkReady);
                chapterAudio.seekTo(startTime);
                chapterAudio.play();
            }
        }, 100);

        // Timeout after 10 seconds
        setTimeout(() => clearInterval(checkReady), 10000);
    }
}

function toggleSentenceSelection(sentenceId) {
    const index = selectedSentences.indexOf(sentenceId);
    const sentenceElem = document.querySelector(`.compact-sentence[data-sentence-id="${sentenceId}"]`);

    if (index === -1) {
        // Add to selection
        selectedSentences.push(sentenceId);
        if (sentenceElem) sentenceElem.classList.add('selected');
    } else {
        // Remove from selection
        selectedSentences.splice(index, 1);
        if (sentenceElem) sentenceElem.classList.remove('selected');
    }

    // If we have 2+ selected, show export button
    updateMultiSelectUI();
}

function updateMultiSelectUI() {
    // No floating button needed - 'e' key handles export
}

function exportSelectedSentences() {
    if (!currentReport || selectedSentences.length === 0) return;

    // Sort selected IDs to ensure sequential order
    const sortedIds = [...selectedSentences].sort((a, b) => a - b);
    const sentences = sortedIds.map(id => currentReport.sentences.find(s => s.id === id)).filter(Boolean);

    if (sentences.length === 0) return;

    const firstSentence = sentences[0];
    const lastSentence = sentences[sentences.length - 1];
    const startTime = firstSentence.startTime;
    const endTime = lastSentence.endTime;

    // Check if any sentence has a diff
    const sentenceWithDiff = sentences.find(s => s.diff && s.diff.ops);

    openCrxModal(currentChapter, startTime, endTime, sentenceWithDiff ? sentenceWithDiff.id : firstSentence.id, null, sentences);

    // Clear selection after export
    clearSentenceSelection();
}

function clearSentenceSelection() {
    selectedSentences.forEach(id => {
        const elem = document.querySelector(`.compact-sentence[data-sentence-id="${id}"]`);
        if (elem) elem.classList.remove('selected');
    });
    selectedSentences = [];
    updateMultiSelectUI();
}

function openCrxForSentence(sentenceId) {
    if (!currentReport) return;
    const sentence = currentReport.sentences.find(s => s.id === sentenceId);
    if (!sentence) return;

    // Don't pass excerpt - let openCrxModal generate the comment from diff data
    openCrxModal(currentChapter, sentence.startTime, sentence.endTime, sentenceId, null);
}

// Errors view
function renderReport(report) {
    currentReport = report;
    const content = document.getElementById('content');

    if (currentView === 'playback') {
        renderPlaybackView(report);
        return;
    }

    // Filter and sort by WER descending for errors view
    const filteredSentences = report.sentences
        .filter(hasErrors)
        .sort((a, b) => {
            const werA = parseFloat(a.wer?.replace('%', '') || 0);
            const werB = parseFloat(b.wer?.replace('%', '') || 0);
            return werB - werA; // Descending order
        });

    const sentencesHtml = filteredSentences.map(s => `
        <div id="sentence-${s.id}" class="sentence-item ${s.status}" data-sentence-id="${s.id}" data-start="${s.startTime}" style="cursor: pointer;">
            <div class="sentence-header">
                <span class="sentence-id">#${s.id}</span>
                <div class="sentence-metrics">
                    <div class="metric">
                        <span class="metric-label">WER:</span>
                        <span class="metric-value ${getMetricClass(s.wer)}">${s.wer}</span>
                    </div>
                    <div class="metric">
                        <span class="metric-label">CER:</span>
                        <span class="metric-value ${getMetricClass(s.cer)}">${s.cer}</span>
                    </div>
                    <span class="status-badge ${s.status}">${s.status}</span>
                </div>
            </div>
            <div class="sentence-details">
                <div>Book range: ${s.bookRange}</div>
                <div>Script range: ${s.scriptRange}</div>
                <div>Timing: ${s.timing}</div>
                ${s.paragraphId !== null && s.paragraphId !== undefined ? `<div class="paragraph-flags">Parent paragraph: <a href="#paragraph-${s.paragraphId}" onclick="focusParagraph(${s.paragraphId}); return false;">#${s.paragraphId}</a></div>` : ''}
            </div>
            <div class="sentence-text">
                ${s.diff && s.diff.ops ? `
                    <div class="text-label">Manuscript</div>
                    <div class="text-content">${escapeHtml(s.bookText)}</div>
                    <div class="text-label" style="margin-top: 12px;">Transcript</div>
                    <div class="diff-unified" data-sentence-id="${s.id}"></div>
                ` : s.scriptText && s.bookText ? `
                    <div class="text-label">Script (Read as)</div>
                    <div class="text-content diff-text" data-sentence-id="${s.id}" data-diff-script="${escapeHtml(s.scriptText)}" data-diff-book="${escapeHtml(s.bookText)}"></div>
                    <div class="text-label" style="margin-top: 12px;">Book (Should be)</div>
                    <div class="text-content diff-text" data-sentence-id="${s.id}" data-diff-script="${escapeHtml(s.scriptText)}" data-diff-book="${escapeHtml(s.bookText)}" data-diff-type="book"></div>
                ` : `
                    <div class="text-label">Book</div>
                    <div class="text-content">${escapeHtml(s.bookText)}</div>
                    ${s.scriptText ? `
                        <div class="text-label" style="margin-top: 12px;">Script</div>
                        <div class="text-content">${escapeHtml(s.scriptText)}</div>
                    ` : ''}
                `}
            </div>
            ${s.startTime !== null && s.endTime !== null ? `
                <div class="action-buttons">
                    <button class="play-button" onclick="playAudioSegment('${currentChapter}', ${s.startTime}, ${s.endTime})">
                        <span class="play-icon"></span>
                        Play Audio Segment
                    </button>
                    <button class="export-button" data-chapter="${currentChapter}" data-start="${s.startTime}" data-end="${s.endTime}" data-sentence-id="${s.id}" data-excerpt="${escapeHtml(s.excerpt || '')}">
                        Export Audio
                    </button>
                    <button class="crx-button" data-chapter="${currentChapter}" data-start="${s.startTime}" data-end="${s.endTime}" data-sentence-id="${s.id}" data-excerpt="${escapeHtml(s.excerpt || '')}">
                        Add to CRX
                    </button>
                </div>
            ` : ''}
        </div>
    `).join('');

    const paragraphsHtml = report.paragraphs.map(p => `
        <div id="paragraph-${p.id}" class="paragraph-item ${p.status}">
            <div class="sentence-header">
                <span class="sentence-id">#${p.id}</span>
                <div class="sentence-metrics">
                    <div class="metric">
                        <span class="metric-label">WER:</span>
                        <span class="metric-value ${getMetricClass(parseFloat(p.wer))}">${p.wer}</span>
                    </div>
                    <div class="metric">
                        <span class="metric-label">Coverage:</span>
                        <span class="metric-value">${p.coverage}</span>
                    </div>
                    <span class="status-badge ${p.status}">${p.status}</span>
                </div>
            </div>
            <div class="sentence-details">
                <div>Book range: ${p.bookRange}</div>
                ${p.timing ? `<div>Timing: ${p.timing}</div>` : ''}
                ${Array.isArray(p.flaggedSentenceIds) && p.flaggedSentenceIds.length ? `<div class="paragraph-flags">Flagged sentences: ${p.flaggedSentenceIds.map(id => `<a href="#sentence-${id}" onclick="focusSentence(${id}); return false;">#${id}</a>`).join(', ')}</div>` : ''}
            </div>
            ${p.bookText ? `
                <div class="sentence-text">
                    <div class="text-label">Book</div>
                    <div class="text-content">${renderParagraphText(p.bookText, p.flaggedSentenceIds || [], report.sentences)}</div>
                </div>
            ` : ''}
            ${p.startTime !== null && p.endTime !== null ? `
                <div class="action-buttons">
                    <button class="play-button" onclick="playAudioSegment('${currentChapter}', ${p.startTime}, ${p.endTime})">
                        <span class="play-icon"></span>
                        Play Paragraph Audio
                    </button>
                </div>
            ` : ''}
        </div>
    `).join('');

    content.innerHTML = `
        <div class="report-header">
            <h1>${report.chapterName}</h1>
            <div class="report-meta">
                <div class="meta-item"><span class="meta-label">Audio:</span> ${report.audioPath}</div>
                <div class="meta-item"><span class="meta-label">Script:</span> ${report.scriptPath}</div>
                <div class="meta-item"><span class="meta-label">Created:</span> ${report.created}</div>
            </div>
        </div>

        <div class="view-toggle">
            <button class="view-toggle-button active" data-view="errors" onclick="switchView('errors')">Errors View</button>
            <button class="view-toggle-button" data-view="playback" onclick="switchView('playback')">Playback View</button>
        </div>

        <div class="stats-grid">
            <div class="stat-card">
                <div class="stat-label">Sentences</div>
                <div class="stat-value">${report.stats.sentenceCount}</div>
                <div class="stat-detail">
                    Avg WER ${report.stats.avgWer} | Max WER ${report.stats.maxWer}
                </div>
                <div class="stat-detail">Flagged: ${report.stats.flaggedCount}</div>
                <div class="stat-detail">With Errors: ${filteredSentences.length}</div>
            </div>
            <div class="stat-card">
                <div class="stat-label">Paragraphs</div>
                <div class="stat-value">${report.stats.paragraphCount}</div>
                <div class="stat-detail">
                    Avg WER ${report.stats.paragraphAvgWer}
                </div>
                <div class="stat-detail">Avg Coverage ${report.stats.avgCoverage}</div>
            </div>
        </div>

        <div class="section-title">Sentences with Errors (${filteredSentences.length})</div>
        ${sentencesHtml}

        <div class="section-title">Paragraphs by WER</div>
        ${paragraphsHtml}
    `;

    setTimeout(() => {
        document.querySelectorAll('.export-button').forEach(btn => {
            btn.addEventListener('click', function() {
                const chapter = this.getAttribute('data-chapter');
                const start = parseFloat(this.getAttribute('data-start'));
                const end = parseFloat(this.getAttribute('data-end'));
                const sentenceId = this.getAttribute('data-sentence-id');
                const excerpt = this.getAttribute('data-excerpt');
                exportAudio(chapter, start, end, sentenceId, excerpt);
            });
        });

        document.querySelectorAll('.crx-button').forEach(btn => {
            btn.addEventListener('click', function() {
                const chapter = this.getAttribute('data-chapter');
                const start = parseFloat(this.getAttribute('data-start'));
                const end = parseFloat(this.getAttribute('data-end'));
                const sentenceId = parseInt(this.getAttribute('data-sentence-id'), 10);
                const excerpt = this.getAttribute('data-excerpt');
                openCrxModal(chapter, start, end, sentenceId, excerpt);
            });
        });

        // Add click listeners to sentence items in errors view
        document.querySelectorAll('.sentence-item').forEach(item => {
            item.addEventListener('click', function(e) {
                // Don't trigger if clicking on buttons or interactive elements
                if (e.target.closest('button') || e.target.closest('a')) {
                    return;
                }
                const sentenceId = parseInt(this.getAttribute('data-sentence-id'), 10);
                const startTime = parseFloat(this.getAttribute('data-start'));
                if (!isNaN(sentenceId) && !isNaN(startTime)) {
                    switchView('playback');
                    // Wait for view to render, then scroll to sentence
                    setTimeout(() => {
                        const targetSentence = document.querySelector(`.compact-sentence[data-sentence-id="${sentenceId}"]`);
                        if (targetSentence) {
                            targetSentence.scrollIntoView({ behavior: 'smooth', block: 'center' });
                            targetSentence.classList.add('highlight-flash');
                            setTimeout(() => targetSentence.classList.remove('highlight-flash'), 2000);
                        }
                        seekToSentence(sentenceId, startTime);
                    }, 100);
                }
            });
        });

        document.querySelectorAll('.diff-text').forEach(elem => {
            const sentenceIdAttr = elem.getAttribute('data-sentence-id');
            const diffType = elem.getAttribute('data-diff-type') || 'script';
            const scriptRaw = elem.getAttribute('data-diff-script');
            const bookRaw = elem.getAttribute('data-diff-book');

            const scriptText = decodeHtml(scriptRaw);
            const bookText = decodeHtml(bookRaw);

            let wordOps = null;
            if (sentenceIdAttr && currentReport && Array.isArray(currentReport.sentences)) {
                const sentenceId = parseInt(sentenceIdAttr, 10);
                const sentenceData = currentReport.sentences.find(s => s.id === sentenceId);
                if (sentenceData && Array.isArray(sentenceData.wordOps)) {
                    wordOps = sentenceData.wordOps;
                }
            }

            if (scriptText || bookText) {
                const highlighted = highlightDifferencesVisual(scriptText, bookText, wordOps);
                elem.innerHTML = diffType === 'book' ? highlighted.bookHighlighted : highlighted.scriptHighlighted;
            }
        });

        document.querySelectorAll('.diff-unified').forEach(elem => {
            const sentenceIdAttr = elem.getAttribute('data-sentence-id');
            if (sentenceIdAttr && currentReport && Array.isArray(currentReport.sentences)) {
                const sentenceId = parseInt(sentenceIdAttr, 10);
                const sentenceData = currentReport.sentences.find(s => s.id === sentenceId);
                if (sentenceData && sentenceData.diff) {
                    elem.innerHTML = renderUnifiedDiff(sentenceData.diff);
                }
            }
        });
    }, 0);
}

// Utility functions
function getMetricClass(value) {
    if (value >= 50) return 'high';
    if (value >= 10) return 'medium';
    return 'low';
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function decodeHtml(html) {
    if (!html) return '';
    const textarea = document.createElement('textarea');
    textarea.innerHTML = html;
    return textarea.value;
}

// Audio playback
function playAudioSegment(chapterName, startTime, endTime) {
    let audioPlayer = document.getElementById('global-audio-player');
    if (!audioPlayer) {
        audioPlayer = document.createElement('audio');
        audioPlayer.id = 'global-audio-player';
        audioPlayer.controls = true;
        audioPlayer.style.display = 'none';
        document.body.appendChild(audioPlayer);
    }

    // Use current audio source
    let url = `/api/audio/${encodeURIComponent(chapterName)}?start=${startTime}&end=${endTime}`;
    if (currentAudioSource !== 'raw') {
        url += `&source=${currentAudioSource}`;
    }

    audioPlayer.src = url;
    audioPlayer.currentTime = 0;
    audioPlayer.play().catch(err => {
        console.error('Failed to play audio:', err);
        alert('Failed to play audio segment. Check console for details.');
    });
}

// Navigation
function focusSentence(sentenceId) {
    const element = document.getElementById(`sentence-${sentenceId}`);
    if (!element) return;

    element.scrollIntoView({ behavior: 'smooth', block: 'center' });
    element.classList.add('highlight');
    setTimeout(() => element.classList.remove('highlight'), 1600);
}

function focusParagraph(paragraphId) {
    const element = document.getElementById(`paragraph-${paragraphId}`);
    if (!element) return;

    element.scrollIntoView({ behavior: 'smooth', block: 'center' });
    element.classList.add('highlight');
    setTimeout(() => element.classList.remove('highlight'), 1600);
}

function renderParagraphText(paragraphText, flaggedSentenceIds, allSentences) {
    if (!flaggedSentenceIds || flaggedSentenceIds.length === 0) {
        return escapeHtml(paragraphText);
    }

    const sentenceMap = {};
    allSentences.forEach(s => {
        if (flaggedSentenceIds.includes(s.id)) {
            sentenceMap[s.id] = s.bookText;
        }
    });

    let result = escapeHtml(paragraphText);

    flaggedSentenceIds.forEach(sentenceId => {
        const sentenceText = sentenceMap[sentenceId];
        if (!sentenceText) return;

        const escapedSentenceText = escapeHtml(sentenceText);
        const regex = new RegExp(escapedSentenceText.replace(/[.*+?^${}()|[\]\\]/g, '\\$&'), 'g');
        result = result.replace(regex, `<a href="#sentence-${sentenceId}" onclick="focusSentence(${sentenceId}); return false;" style="color: #4ec9b0; text-decoration: none;">${escapedSentenceText}</a>`);
    });

    return result;
}

// Export functions
async function exportAudio(chapterName, startTime, endTime, sentenceId, excerpt) {
    try {
        const response = await fetch(`/api/export/${encodeURIComponent(chapterName)}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                start: startTime,
                end: endTime,
                sentenceId: sentenceId,
                excerpt: excerpt
            })
        });

        const result = await response.json();

        if (result.success) {
            showToast(`Audio exported: ${result.filename}<br>Error #${result.errorNumber}`, 'success');
        } else {
            showToast(`Export failed: ${result.error}`, 'error');
        }
    } catch (err) {
        console.error('Failed to export audio:', err);
        showToast('Failed to export audio. Check console for details.', 'error');
    }
}

// CRX Modal
function generateCrxComment(sentence) {
    if (!sentence) return '';

    // Use diff data if available
    if (sentence.diff && sentence.diff.ops) {
        return generateBracketedDiffComment(sentence.diff, sentence.bookText);
    }

    // Fallback to old method
    const diff = highlightDifferences(sentence.scriptText || '', sentence.bookText || '', sentence.wordOps);
    return `Read as: ${diff.scriptHighlighted}\nShould be: ${diff.bookHighlighted}`;
}

function generateCrxCommentWithSelection(sentence) {
    if (!sentence || !sentence.diff || !sentence.diff.ops) {
        return generateCrxComment(sentence);
    }

    // Get selected error indices
    const selectedIndices = new Set();
    document.querySelectorAll('#errorCheckboxes input[type="checkbox"]:checked').forEach(cb => {
        selectedIndices.add(parseInt(cb.dataset.errorIndex));
    });

    // Build comment with only selected errors bracketed
    let shouldBeLine = [];
    let readAsLine = [];
    let hasDifferences = false;

    sentence.diff.ops.forEach((op, index) => {
        const tokens = op.tokens || [];
        if (tokens.length === 0) return;

        const text = tokens.join(' ');
        const isSelected = selectedIndices.has(index);

        if (op.op === 'equal') {
            shouldBeLine.push(text);
            readAsLine.push(text);
        } else if (op.op === 'delete') {
            if (isSelected) {
                shouldBeLine.push(`[${text}]`);
                hasDifferences = true;
            } else {
                shouldBeLine.push(text);
                readAsLine.push(text);
            }
        } else if (op.op === 'insert') {
            if (isSelected) {
                readAsLine.push(`[${text}]`);
                hasDifferences = true;
            } else {
                shouldBeLine.push(text);
                readAsLine.push(text);
            }
        }
    });

    if (!hasDifferences) {
        return sentence.bookText;
    }

    const shouldBe = shouldBeLine.join(' ');
    const readAs = readAsLine.join(' ');

    return `Should be: ${shouldBe}\nRead as: ${readAs}`;
}

function generateBracketedDiffComment(diff, bookText) {
    // Build two lines showing differences in brackets
    let shouldBeLine = [];
    let readAsLine = [];
    let hasDifferences = false;

    diff.ops.forEach(op => {
        const tokens = op.tokens || [];
        if (tokens.length === 0) return;

        const text = tokens.join(' ');

        if (op.op === 'equal') {
            // Same in both
            shouldBeLine.push(text);
            readAsLine.push(text);
        } else if (op.op === 'delete') {
            // In book but not read
            shouldBeLine.push(`[${text}]`);
            hasDifferences = true;
            // Don't add to readAs
        } else if (op.op === 'insert') {
            // Read but not in book
            readAsLine.push(`[${text}]`);
            hasDifferences = true;
            // Don't add to shouldBe
        }
    });

    // If no differences, just return the sentence
    if (!hasDifferences) {
        return bookText;
    }

    const shouldBe = shouldBeLine.join(' ');
    const readAs = readAsLine.join(' ');

    return `Should be: ${shouldBe}\n\nRead as: ${readAs}`;
}

function openCrxModal(chapterName, startTime, endTime, sentenceId, excerpt, sentences = null) {
    const decodedExcerpt = decodeHtml(excerpt || '');

    pendingCrxData = {
        chapterName: chapterName,
        start: startTime,
        end: endTime,
        sentenceId: sentenceId,
        excerpt: decodedExcerpt,
        sentence: null,
        sentences: sentences  // Store multiple sentences if provided
    };

    const sentence = currentReport && Array.isArray(currentReport.sentences)
        ? currentReport.sentences.find(s => s.id === sentenceId)
        : null;

    pendingCrxData.sentence = sentence;

    // Populate error checkboxes from diff data
    populateErrorCheckboxes(sentence);

    // Always try to generate comment from sentence diff data first
    const commentsField = document.getElementById('comments');

    if (sentences && sentences.length > 1) {
        // Generate concatenated comment for multiple sentences
        const shouldBeLines = [];
        const readAsLines = [];

        sentences.forEach(s => {
            if (s.diff && s.diff.ops) {
                const shouldBeLine = [];
                const readAsLine = [];

                s.diff.ops.forEach(op => {
                    const tokens = op.tokens || [];
                    const text = tokens.join(' ');

                    if (op.op === 'equal') {
                        shouldBeLine.push(text);
                        readAsLine.push(text);
                    } else if (op.op === 'delete') {
                        shouldBeLine.push(`[${text}]`);
                    } else if (op.op === 'insert') {
                        readAsLine.push(`[${text}]`);
                    }
                });

                if (shouldBeLine.length > 0) shouldBeLines.push(shouldBeLine.join(' '));
                if (readAsLine.length > 0) readAsLines.push(readAsLine.join(' '));
            } else {
                // No diff, just add book text
                const text = s.bookText || '';
                if (text) {
                    shouldBeLines.push(text);
                    readAsLines.push(text);
                }
            }
        });

        const shouldBe = shouldBeLines.join(' ');
        const readAs = readAsLines.join(' ');

        commentsField.value = `Should be: ${shouldBe}\n\nRead as: ${readAs}`;
    } else if (sentence) {
        commentsField.value = generateCrxComment(sentence);
    } else if (decodedExcerpt) {
        commentsField.value = decodedExcerpt;
    } else {
        commentsField.value = '';
    }

    // Show modal first, then initialize audio after DOM is ready
    document.getElementById('crxModal').style.display = 'block';

    // Delay audio preview initialization to let modal render
    setTimeout(() => {
        refreshAudioPreview();
    }, 50);
}

function populateErrorCheckboxes(sentence) {
    const container = document.getElementById('errorCheckboxes');
    container.innerHTML = '';

    if (!sentence || !sentence.diff || !sentence.diff.ops) {
        container.innerHTML = '<div style="color: #858585; font-style: italic;">No diff data available</div>';
        document.getElementById('selectAllErrors').disabled = true;
        return;
    }

    document.getElementById('selectAllErrors').disabled = false;
    document.getElementById('selectAllErrors').checked = true;

    // Collect all non-equal ops
    const errors = [];
    sentence.diff.ops.forEach((op, index) => {
        if (op.op !== 'equal' && op.tokens && op.tokens.length > 0) {
            errors.push({
                index: index,
                type: op.op,
                text: op.tokens.join(' ')
            });
        }
    });

    if (errors.length === 0) {
        container.innerHTML = '<div style="color: #858585; font-style: italic;">No errors detected</div>';
        document.getElementById('selectAllErrors').disabled = true;
        return;
    }

    errors.forEach(error => {
        const label = document.createElement('label');
        label.style.display = 'block';
        label.style.marginBottom = '4px';

        const checkbox = document.createElement('input');
        checkbox.type = 'checkbox';
        checkbox.checked = true;
        checkbox.dataset.errorIndex = error.index;
        checkbox.onchange = () => updateCrxComment();

        const typeSpan = document.createElement('span');
        typeSpan.style.color = error.type === 'delete' ? '#ff6b6b' : '#69db7c';
        typeSpan.style.fontWeight = '600';
        typeSpan.textContent = error.type === 'delete' ? 'DEL' : 'INS';

        label.appendChild(checkbox);
        label.appendChild(document.createTextNode(' '));
        label.appendChild(typeSpan);
        label.appendChild(document.createTextNode(`: "${error.text}"`));

        container.appendChild(label);
    });
}

function toggleAllErrors(checked) {
    const checkboxes = document.querySelectorAll('#errorCheckboxes input[type="checkbox"]');
    checkboxes.forEach(cb => cb.checked = checked);
    updateCrxComment();
}

function updatePaddingValue(value) {
    document.getElementById('paddingValue').textContent = value;
}

function updateCrxComment() {
    if (!pendingCrxData || !pendingCrxData.sentence) return;

    const commentsField = document.getElementById('comments');
    commentsField.value = generateCrxCommentWithSelection(pendingCrxData.sentence);
}

function refreshAudioPreview() {
    if (!pendingCrxData) return;

    const padding = parseInt(document.getElementById('paddingSlider').value) / 1000; // Convert ms to seconds
    const start = pendingCrxData.start;
    const end = pendingCrxData.end + padding;

    // Destroy existing WaveSurfer instance if any
    if (waveSurferInstances['crx-audio-preview-container']) {
        waveSurferInstances['crx-audio-preview-container'].destroy();
    }

    // Create new WaveSurfer player
    const audioUrl = `/api/audio/${encodeURIComponent(pendingCrxData.chapterName)}?start=${start}&end=${end}`;
    const player = new WaveSurferPlayer('crx-audio-preview-container', audioUrl, {
        title: 'Audio Preview',
        height: 60,
        waveColor: '#4a9eff',
        progressColor: '#1177bb',
        fillParent: false,
        hideScrollbar: false,
        scrollParent: true, 
        autoCenter: true,
        minPxPerSec: 100,
        disableZoom: true
    });
    player.render();
}

function closeCrxModal() {
    // Destroy WaveSurfer instance
    if (waveSurferInstances['crx-audio-preview-container']) {
        waveSurferInstances['crx-audio-preview-container'].destroy();
    }

    document.getElementById('crxModal').style.display = 'none';
    pendingCrxData = null;
}

async function submitCrx() {
    if (!pendingCrxData) return;

    const errorType = document.getElementById('errorType').value;
    const comments = document.getElementById('comments').value;
    const paddingMs = parseInt(document.getElementById('paddingSlider').value);

    try {
        const response = await fetch(`/api/crx/${encodeURIComponent(pendingCrxData.chapterName)}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                start: pendingCrxData.start,
                end: pendingCrxData.end,
                sentenceId: pendingCrxData.sentenceId,
                errorType: errorType,
                comments: comments,
                excerpt: pendingCrxData.excerpt,
                paddingMs: paddingMs
            })
        });

        const result = await response.json();

        if (result.success) {
            showToast(`Added to CRX!<br>Error #${result.errorNumber} • ${result.timecode}<br>Audio: ${result.audioFile}`, 'success');
            closeCrxModal();
        } else {
            showToast(`Failed to add to CRX: ${result.error}`, 'error');
        }
    } catch (err) {
        console.error('Failed to add to CRX:', err);
        showToast('Failed to add to CRX. Check console for details.', 'error');
    }
}

// Diff highlighting
function buildDiffFromWordOps(wordOps, options) {
    if (!Array.isArray(wordOps) || wordOps.length === 0) {
        return null;
    }

    const scriptParts = [];
    const bookParts = [];

    const pushScript = (word, highlighted) => {
        if (!word) return;
        scriptParts.push(highlighted ? options.highlight(word) : options.normal(word));
    };

    const pushBook = (word, highlighted) => {
        if (!word) return;
        bookParts.push(highlighted ? options.highlight(word) : options.normal(word));
    };

    wordOps.forEach(op => {
        const opType = (op.op || '').toLowerCase();
        const scriptWord = (op.asrWord || '').trim();
        const bookWord = (op.bookWord || '').trim();

        switch (opType) {
            case 'match':
                pushScript(scriptWord, false);
                pushBook(bookWord, false);
                break;
            case 'sub':
                pushScript(scriptWord, true);
                pushBook(bookWord, true);
                break;
            case 'del':
                pushBook(bookWord, true);
                break;
            case 'ins':
                pushScript(scriptWord, true);
                break;
            default:
                pushScript(scriptWord, false);
                pushBook(bookWord, false);
                break;
        }
    });

    return {
        script: scriptParts.join(' ').replace(/\s+/g, ' ').trim(),
        book: bookParts.join(' ').replace(/\s+/g, ' ').trim()
    };
}

function highlightDifferences(script, book, wordOps = null) {
    const diffFromOps = buildDiffFromWordOps(wordOps, {
        normal: word => word,
        highlight: word => `[${word}]`
    });

    if (diffFromOps) {
        return {
            scriptHighlighted: diffFromOps.script,
            bookHighlighted: diffFromOps.book
        };
    }

    const scriptWords = script.split(/\s+/);
    const bookWords = book.split(/\s+/);

    let scriptResult = [];
    let bookResult = [];
    let scriptMismatchBuffer = [];
    let bookMismatchBuffer = [];

    function normalizeWord(word) {
        return word.replace(/[.,!?;:'"()\-—–]/g, '').toLowerCase();
    }

    function flushBuffers() {
        if (scriptMismatchBuffer.length > 0) {
            scriptResult.push(`[${scriptMismatchBuffer.join(' ')}]`);
            scriptMismatchBuffer = [];
        }
        if (bookMismatchBuffer.length > 0) {
            bookResult.push(`[${bookMismatchBuffer.join(' ')}]`);
            bookMismatchBuffer = [];
        }
    }

    let i = 0, j = 0;

    while (i < scriptWords.length || j < bookWords.length) {
        const scriptWord = scriptWords[i] || '';
        const bookWord = bookWords[j] || '';

        const scriptNorm = normalizeWord(scriptWord);
        const bookNorm = normalizeWord(bookWord);

        if (scriptNorm === bookNorm) {
            flushBuffers();
            if (scriptWord) scriptResult.push(scriptWord);
            if (bookWord) bookResult.push(bookWord);
            i++;
            j++;
        } else {
            let matched = false;
            for (let n = 1; n <= 3 && i + n <= scriptWords.length; n++) {
                const scriptPhrase = scriptWords.slice(i, i + n).join('');
                const scriptPhraseNorm = normalizeWord(scriptPhrase);
                if (scriptPhraseNorm === bookNorm) {
                    flushBuffers();
                    scriptResult.push(scriptWords.slice(i, i + n).join(' '));
                    bookResult.push(bookWord);
                    i += n;
                    j++;
                    matched = true;
                    break;
                }
            }

            if (!matched) {
                for (let n = 1; n <= 3 && j + n <= bookWords.length; n++) {
                    const bookPhrase = bookWords.slice(j, j + n).join('');
                    const bookPhraseNorm = normalizeWord(bookPhrase);
                    if (bookPhraseNorm === scriptNorm) {
                        flushBuffers();
                        scriptResult.push(scriptWord);
                        bookResult.push(bookWords.slice(j, j + n).join(' '));
                        i++;
                        j += n;
                        matched = true;
                        break;
                    }
                }
            }

            if (!matched) {
                if (scriptWord) scriptMismatchBuffer.push(scriptWord);
                if (bookWord) bookMismatchBuffer.push(bookWord);
                if (i < scriptWords.length) i++;
                if (j < bookWords.length) j++;
            }
        }
    }

    flushBuffers();

    return {
        scriptHighlighted: scriptResult.join(' '),
        bookHighlighted: bookResult.join(' ')
    };
}

function highlightDifferencesVisual(script, book, wordOps = null) {
    const diffFromOps = buildDiffFromWordOps(wordOps, {
        normal: word => escapeHtml(word),
        highlight: word => `<mark class="diff-highlight">${escapeHtml(word)}</mark>`
    });

    if (diffFromOps) {
        return {
            scriptHighlighted: diffFromOps.script,
            bookHighlighted: diffFromOps.book
        };
    }

    const scriptWords = script.split(/\s+/);
    const bookWords = book.split(/\s+/);

    let scriptResult = [];
    let bookResult = [];
    let scriptMismatchBuffer = [];
    let bookMismatchBuffer = [];

    function normalizeWord(word) {
        return word.replace(/[.,!?;:'"()\-—–]/g, '').toLowerCase();
    }

    function flushBuffers() {
        if (scriptMismatchBuffer.length > 0) {
            scriptResult.push(`<mark class="diff-highlight">${escapeHtml(scriptMismatchBuffer.join(' '))}</mark>`);
            scriptMismatchBuffer = [];
        }
        if (bookMismatchBuffer.length > 0) {
            bookResult.push(`<mark class="diff-highlight">${escapeHtml(bookMismatchBuffer.join(' '))}</mark>`);
            bookMismatchBuffer = [];
        }
    }

    let i = 0, j = 0;

    while (i < scriptWords.length || j < bookWords.length) {
        const scriptWord = scriptWords[i] || '';
        const bookWord = bookWords[j] || '';

        const scriptNorm = normalizeWord(scriptWord);
        const bookNorm = normalizeWord(bookWord);

        if (scriptNorm === bookNorm) {
            flushBuffers();
            if (scriptWord) scriptResult.push(escapeHtml(scriptWord));
            if (bookWord) bookResult.push(escapeHtml(bookWord));
            i++;
            j++;
        } else {
            let matched = false;
            for (let n = 1; n <= 3 && i + n <= scriptWords.length; n++) {
                const scriptPhrase = scriptWords.slice(i, i + n).join('');
                const scriptPhraseNorm = normalizeWord(scriptPhrase);
                if (scriptPhraseNorm === bookNorm) {
                    flushBuffers();
                    scriptResult.push(escapeHtml(scriptWords.slice(i, i + n).join(' ')));
                    bookResult.push(escapeHtml(bookWord));
                    i += n;
                    j++;
                    matched = true;
                    break;
                }
            }

            if (!matched) {
                for (let n = 1; n <= 3 && j + n <= bookWords.length; n++) {
                    const bookPhrase = bookWords.slice(j, j + n).join('');
                    const bookPhraseNorm = normalizeWord(bookPhrase);
                    if (bookPhraseNorm === scriptNorm) {
                        flushBuffers();
                        scriptResult.push(escapeHtml(scriptWord));
                        bookResult.push(escapeHtml(bookWords.slice(j, j + n).join(' ')));
                        i++;
                        j += n;
                        matched = true;
                        break;
                    }
                }
            }

            if (!matched) {
                if (scriptWord) scriptMismatchBuffer.push(scriptWord);
                if (bookWord) bookMismatchBuffer.push(bookWord);
                if (i < scriptWords.length) i++;
                if (j < bookWords.length) j++;
            }
        }
    }

    flushBuffers();

    return {
        scriptHighlighted: scriptResult.join(' '),
        bookHighlighted: bookResult.join(' ')
    };
}

function renderUnifiedDiff(diff) {
    if (!diff || !diff.ops || !Array.isArray(diff.ops)) {
        return '<div class="diff-empty">No diff data available</div>';
    }

    const parts = [];
    let i = 0;

    while (i < diff.ops.length) {
        const op = diff.ops[i];
        const tokens = op.tokens || [];

        if (tokens.length === 0) {
            i++;
            continue;
        }

        const text = tokens.join(' ');

        if (op.op === 'equal') {
            parts.push(escapeHtml(text));
            i++;
        } else if (op.op === 'delete') {
            // Collect consecutive deletes
            const deletedTexts = [text];
            let j = i + 1;
            while (j < diff.ops.length && diff.ops[j].op === 'delete') {
                const delTokens = diff.ops[j].tokens || [];
                if (delTokens.length > 0) {
                    deletedTexts.push(delTokens.join(' '));
                }
                j++;
            }

            // Check if followed by inserts (substitution)
            const insertedTexts = [];
            let k = j;
            while (k < diff.ops.length && diff.ops[k].op === 'insert') {
                const insTokens = diff.ops[k].tokens || [];
                if (insTokens.length > 0) {
                    insertedTexts.push(insTokens.join(' '));
                }
                k++;
            }

            if (insertedTexts.length > 0) {
                // Substitution: show as deleted → inserted
                const deletedStr = deletedTexts.map(t => escapeHtml(t)).join(' ');
                const insertedStr = insertedTexts.map(t => escapeHtml(t)).join(' ');
                parts.push(`<span class="diff-substitution"><span class="diff-deleted">${deletedStr}</span> → <span class="diff-inserted">${insertedStr}</span></span>`);
                i = k;
            } else {
                // Pure deletion
                const deletedStr = deletedTexts.map(t => escapeHtml(t)).join(' ');
                parts.push(`<span class="diff-deleted">${deletedStr}</span>`);
                i = j;
            }
        } else if (op.op === 'insert') {
            // Pure insertion (not part of a substitution)
            parts.push(`<span class="diff-inserted">${escapeHtml(text)}</span>`);
            i++;
        } else {
            i++;
        }
    }

    if (parts.length === 0) {
        return '<div class="diff-empty">No transcript data</div>';
    }

    return '<div class="diff-inline">' + parts.join(' ') + '</div>';
}

// Toast notifications
function showToast(message, type = 'success', duration = 5000) {
    const container = document.getElementById('toast-container');
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;

    const title = type === 'success' ? 'Success' : 'Error';

    toast.innerHTML = `
        <div class="toast-header">
            <div class="toast-title">${title}</div>
            <button class="toast-close" onclick="this.parentElement.parentElement.remove()">×</button>
        </div>
        <div class="toast-message">${message}</div>
    `;

    container.appendChild(toast);

    setTimeout(() => {
        toast.style.animation = 'slideIn 0.3s ease-out reverse';
        setTimeout(() => toast.remove(), 300);
    }, duration);
}

// Modal close handler
window.onclick = function(event) {
    const modal = document.getElementById('crxModal');
    if (event.target === modal) {
        closeCrxModal();
    }
}

// Keyboard shortcuts
document.addEventListener('keydown', (e) => {
    // Check if CRX modal is open
    const modal = document.getElementById('crxModal');
    const modalOpen = modal && modal.style.display === 'block';

    // If modal is open and spacebar pressed, play/pause modal audio instead
    if (modalOpen && (e.code === 'Space' || e.key === ' ')) {
        e.preventDefault();
        const crxPlayer = waveSurferInstances['crx-audio-preview-container'];
        if (crxPlayer && crxPlayer.playPause) {
            crxPlayer.playPause();
        }
        return;
    }

    // Don't trigger shortcuts when typing in inputs/textareas
    if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA') {
        return;
    }

    // Spacebar: play/pause chapter audio (global, prevents scrolling)
    if (e.code === 'Space' || e.key === ' ') {
        e.preventDefault();
        if (chapterAudio && chapterAudio.playPause) {
            chapterAudio.playPause();
        }
    }

    // 'e' key: export to CRX - handles both single and multi-select
    if (e.key === 'e' || e.key === 'E') {
        e.preventDefault();
        if (selectedSentences.length > 0) {
            // Export selected sentences
            exportSelectedSentences();
        } else if (currentPlayingSentence !== null) {
            openCrxForSentence(currentPlayingSentence);
        } else if (window.lastHoveredSentence !== null) {
            openCrxForSentence(window.lastHoveredSentence);
        }
    }

    // 'q' key: close CRX modal if open
    if (e.key === 'q' || e.key === 'Q') {
        if (modalOpen) {
            e.preventDefault();
            closeCrxModal();
        }
    }
});

// Mobile UI functions
function createMobileHeader() {
    if (window.innerWidth > 768) return;

    const existingHeader = document.querySelector('.mobile-header');
    if (existingHeader) return;

    const header = document.createElement('div');
    header.className = 'mobile-header';
    header.innerHTML = `
        <div class="mobile-logo" onclick="loadOverview()">
            <i class="fas fa-book"></i>
            <span>Book Overview</span>
        </div>
        <button class="mobile-chapter-toggle" onclick="toggleMobileChapters()">
            <i class="fas fa-chevron-down"></i>
        </button>
    `;

    document.body.insertBefore(header, document.body.firstChild);

    // Create chapters dropdown
    const dropdown = document.createElement('div');
    dropdown.className = 'mobile-chapters-dropdown';
    dropdown.id = 'mobile-chapters-dropdown';
    document.body.insertBefore(dropdown, document.getElementById('sidebar'));
}

function toggleMobileChapters() {
    const dropdown = document.getElementById('mobile-chapters-dropdown');
    const toggle = document.querySelector('.mobile-chapter-toggle i');

    if (dropdown.classList.contains('open')) {
        dropdown.classList.remove('open');
        toggle.className = 'fas fa-chevron-down';
    } else {
        dropdown.classList.add('open');
        toggle.className = 'fas fa-chevron-up';

        // Populate with chapters
        dropdown.innerHTML = chapters.map(chapter => {
            const isReviewed = reviewedStatus[chapter.name]?.reviewed;
            const reviewedClass = isReviewed ? 'reviewed' : '';
            return `
                <div class="chapter-item ${chapter.name === currentChapter ? 'active' : ''} ${reviewedClass}"
                     onclick="loadReport('${chapter.name}'); toggleMobileChapters();">
                    <div class="chapter-name">${chapter.name}</div>
                    <div class="chapter-stats">Flagged: ${chapter.metrics.sentenceFlagged} sentences</div>
                </div>
            `;
        }).join('');
    }
}

function updateFloatingButton() {
    const isMobile = window.innerWidth <= 768;

    if (isMobile) {
        // Mobile: create bottom action bar if it doesn't exist
        let actionBar = document.querySelector('.mobile-action-bar');
        if (!actionBar) {
            actionBar = document.createElement('div');
            actionBar.className = 'mobile-action-bar';
            document.body.appendChild(actionBar);
        }

        actionBar.innerHTML = `
            <div class="action-bar">
                <button class="action-bar-button" onclick="switchView('errors')">
                    <i class="fas fa-times-circle"></i>
                    <span class="button-text">Errors</span>
                </button>
                <button class="action-bar-button" onclick="switchView('playback')">
                    <i class="fas fa-headphones"></i>
                    <span class="button-text">Playback</span>
                </button>
                <button class="action-bar-button" onclick="showAudioSourceMenu()">
                    <i class="fas fa-volume-up"></i>
                    <span class="button-text">Source</span>
                </button>
                <button class="action-bar-button ${reviewedStatus[currentChapter] ? 'reviewed' : ''}"
                        onclick="toggleReviewStatus('${currentChapter}')">
                    <i class="fas fa-check"></i>
                    <span class="button-text">Review</span>
                </button>
            </div>
        `;
    } else {
        // Desktop: create floating action bar
        const existingBar = document.getElementById('floating-action-bar');
        if (existingBar) {
            existingBar.remove();
        }

        if (!currentChapter) return;

        const isReviewed = reviewedStatus[currentChapter]?.reviewed;
        const reviewedClass = isReviewed ? 'reviewed' : '';
        const buttonText = isReviewed ? '✓ Reviewed' : 'Mark as Reviewed';

        // Create action bar container
        const actionBar = document.createElement('div');
        actionBar.id = 'floating-action-bar';
        actionBar.className = 'floating-action-bar';

        // View switcher group
        const viewGroup = document.createElement('div');
        viewGroup.className = 'action-bar-group';

        const errorsButton = document.createElement('button');
        errorsButton.className = `action-bar-button ${currentView === 'errors' ? 'active' : ''}`;
        errorsButton.innerHTML = 'Errors';
        errorsButton.onclick = () => switchView('errors');

        const playbackButton = document.createElement('button');
        playbackButton.className = `action-bar-button ${currentView === 'playback' ? 'active' : ''}`;
        playbackButton.innerHTML = 'Playback';
        playbackButton.onclick = () => switchView('playback');

        viewGroup.appendChild(errorsButton);
        viewGroup.appendChild(playbackButton);

        // Audio source selector
        const audioSourceSelector = document.createElement('div');
        audioSourceSelector.className = 'action-bar-group';
        audioSourceSelector.style.marginLeft = '8px';

        const audioButton = document.createElement('button');
        audioButton.className = 'action-bar-button audio-source-button';
        audioButton.innerHTML = `<i class="fas fa-volume-up"></i> ${currentAudioSource.charAt(0).toUpperCase() + currentAudioSource.slice(1)}`;
        audioButton.onclick = () => toggleAudioSourceMenu();
        audioButton.id = 'audio-source-button';

        audioSourceSelector.appendChild(audioButton);

        // Separator
        const separator = document.createElement('div');
        separator.className = 'action-bar-separator';

        // Reviewed button
        const reviewedButton = document.createElement('button');
        reviewedButton.className = `action-bar-button reviewed-button ${reviewedClass}`;
        reviewedButton.innerHTML = buttonText;
        reviewedButton.onclick = () => markReviewed(currentChapter, !isReviewed);

        // Assemble action bar
        actionBar.appendChild(viewGroup);
        actionBar.appendChild(audioSourceSelector);
        actionBar.appendChild(separator);
        actionBar.appendChild(reviewedButton);

        document.body.appendChild(actionBar);
    }
}

// Mobile header auto-hide on scroll
function setupMobileHeaderAutoHide() {
    if (window.innerWidth > 768) return;

    let lastScrollY = 0;
    let ticking = false;

    const header = document.querySelector('.mobile-header');
    const dropdown = document.getElementById('mobile-chapters-dropdown');
    const content = document.getElementById('content');

    if (!header || !content) return;

    content.addEventListener('scroll', () => {
        if (!ticking) {
            window.requestAnimationFrame(() => {
                const currentScrollY = content.scrollTop;

                // Don't hide if dropdown is open
                if (dropdown && dropdown.classList.contains('open')) {
                    ticking = false;
                    return;
                }

                if (currentScrollY > lastScrollY && currentScrollY > 60) {
                    // Scrolling down - hide header
                    header.style.transform = 'translateY(-100%)';
                    content.style.paddingTop = '0';
                } else if (currentScrollY < lastScrollY) {
                    // Scrolling up - show header
                    header.style.transform = 'translateY(0)';
                    content.style.paddingTop = '60px';
                }

                lastScrollY = currentScrollY;
                ticking = false;
            });

            ticking = true;
        }
    });
}

// Initialize on page load
loadChapters().then(() => {
    loadOverview();
    createMobileHeader();
    setupMobileHeaderAutoHide();
});

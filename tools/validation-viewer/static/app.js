// Global state
let chapters = [];
let currentChapter = null;
let currentReport = null;
let pendingCrxData = null;
let reviewedStatus = {};
let currentView = 'errors'; // 'errors' or 'playback'
let chapterAudio = null;
let currentPlayingSentence = null;

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

function updateFloatingButton() {
    const existingButton = document.getElementById('floating-reviewed-button');
    if (existingButton) {
        existingButton.remove();
    }

    if (!currentChapter) return;

    const isReviewed = reviewedStatus[currentChapter]?.reviewed;
    const reviewedClass = isReviewed ? 'reviewed' : '';
    const buttonText = isReviewed ? '✓ Reviewed' : 'Mark as Reviewed';

    const button = document.createElement('button');
    button.id = 'floating-reviewed-button';
    button.className = `reviewed-button ${reviewedClass}`;
    button.innerHTML = buttonText;
    button.onclick = () => markReviewed(currentChapter, !isReviewed);

    document.body.appendChild(button);
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
    currentView = view;
    document.querySelectorAll('.view-toggle-button').forEach(btn => btn.classList.remove('active'));
    document.querySelector(`[data-view="${view}"]`).classList.add('active');

    if (currentReport) {
        renderReport(currentReport);
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
                 data-end="${s.endTime}"
                 onclick="seekToSentence(${s.id}, ${s.startTime})"
                 ondblclick="openCrxForSentence(${s.id})">
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

        <div class="chapter-audio-player">
            <audio id="chapter-audio" controls>
                <source src="/api/audio/${encodeURIComponent(currentChapter)}" type="audio/wav">
            </audio>
        </div>

        <div class="section-title">Chapter Sentences (Click to seek, Double-click for CRX)</div>
        ${compactSentencesHtml}
    `;

    setupChapterAudio();
}

function formatTime(seconds) {
    const mins = Math.floor(seconds / 60);
    const secs = Math.floor(seconds % 60);
    return `${mins}:${secs.toString().padStart(2, '0')}`;
}

function setupChapterAudio() {
    chapterAudio = document.getElementById('chapter-audio');
    if (!chapterAudio) return;

    chapterAudio.addEventListener('timeupdate', () => {
        const currentTime = chapterAudio.currentTime;
        updatePlayingSentence(currentTime);
    });
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
    chapterAudio.currentTime = startTime;
    chapterAudio.play();
}

function openCrxForSentence(sentenceId) {
    if (!currentReport) return;
    const sentence = currentReport.sentences.find(s => s.id === sentenceId);
    if (!sentence) return;

    openCrxModal(currentChapter, sentence.startTime, sentence.endTime, sentenceId, sentence.bookText || '');
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
        <div id="sentence-${s.id}" class="sentence-item ${s.status}">
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

    const url = `/api/audio/${encodeURIComponent(chapterName)}?start=${startTime}&end=${endTime}`;
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

function generateBracketedDiffComment(diff, bookText) {
    // Build two lines showing differences in brackets
    let shouldBeLine = [];
    let readAsLine = [];

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
            // Don't add to readAs
        } else if (op.op === 'insert') {
            // Read but not in book
            readAsLine.push(`[${text}]`);
            // Don't add to shouldBe
        }
    });

    const shouldBe = shouldBeLine.join(' ');
    const readAs = readAsLine.join(' ');

    return `Should be: ${shouldBe}\nRead as: ${readAs}`;
}

function openCrxModal(chapterName, startTime, endTime, sentenceId, excerpt) {
    const decodedExcerpt = decodeHtml(excerpt || '');

    pendingCrxData = {
        chapterName: chapterName,
        start: startTime,
        end: endTime,
        sentenceId: sentenceId,
        excerpt: decodedExcerpt
    };

    const commentsField = document.getElementById('comments');
    const sentence = currentReport && Array.isArray(currentReport.sentences)
        ? currentReport.sentences.find(s => s.id === sentenceId)
        : null;

    if (decodedExcerpt) {
        commentsField.value = decodedExcerpt;
    } else if (sentence) {
        commentsField.value = generateCrxComment(sentence);
    } else {
        commentsField.value = '';
    }

    document.getElementById('crxModal').style.display = 'block';
}

function closeCrxModal() {
    document.getElementById('crxModal').style.display = 'none';
    pendingCrxData = null;
}

async function submitCrx() {
    if (!pendingCrxData) return;

    const errorType = document.getElementById('errorType').value;
    const comments = document.getElementById('comments').value;

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
                excerpt: pendingCrxData.excerpt
            })
        });

        const result = await response.json();

        if (result.success) {
            showToast(`Added to CRX!<br>Error #${result.errorNumber} • ${result.timecode}`, 'success');
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

    diff.ops.forEach(op => {
        const tokens = op.tokens || [];

        if (tokens.length === 0) return;

        const text = tokens.join(' ');

        if (op.op === 'equal') {
            parts.push(escapeHtml(text));
        } else if (op.op === 'delete') {
            parts.push(`<span class="diff-deleted">${escapeHtml(text)}</span>`);
        } else if (op.op === 'insert') {
            parts.push(`<span class="diff-inserted">${escapeHtml(text)}</span>`);
        }
    });

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

// Initialize on page load
loadChapters().then(() => {
    loadOverview();
});

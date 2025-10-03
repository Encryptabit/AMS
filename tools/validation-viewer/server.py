#!/usr/bin/env python3
"""
Validation Report Viewer Server
Serves validation reports from chapter directories with a web UI
"""

import os
import sys
import json
import re
from pathlib import Path
from http.server import HTTPServer, BaseHTTPRequestHandler
from urllib.parse import parse_qs, urlparse

# Default configuration
BASE_DIR = Path(r"C:\Aethon\InProgress\Vain Glory 2")
PORT = 8081


class ValidationReportHandler(BaseHTTPRequestHandler):
    def do_GET(self):
        parsed_path = urlparse(self.path)
        path = parsed_path.path

        if path == '/':
            self.serve_index()
        elif path == '/api/chapters':
            self.serve_chapters_list()
        elif path.startswith('/api/report/'):
            chapter_name = path[len('/api/report/'):]
            self.serve_report(chapter_name)
        elif path.startswith('/api/audio/'):
            # Extract chapter name and timing from path
            remainder = path[len('/api/audio/'):]
            self.serve_audio_segment(remainder, parsed_path.query)
        else:
            self.send_error(404)

    def serve_index(self):
        """Serve the main HTML page"""
        html = """<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Validation Report Viewer</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            background: #1e1e1e;
            color: #d4d4d4;
            height: 100vh;
            display: flex;
        }

        #sidebar {
            width: 280px;
            background: #252526;
            border-right: 1px solid #3e3e42;
            overflow-y: auto;
            flex-shrink: 0;
        }

        #sidebar h2 {
            padding: 20px;
            font-size: 16px;
            font-weight: 600;
            color: #cccccc;
            border-bottom: 1px solid #3e3e42;
            background: #2d2d30;
        }

        .chapter-item {
            padding: 12px 20px;
            cursor: pointer;
            border-bottom: 1px solid #3e3e42;
            transition: background 0.2s;
        }

        .chapter-item:hover {
            background: #2a2d2e;
        }

        .chapter-item.active {
            background: #094771;
            border-left: 3px solid #007acc;
        }

        .chapter-name {
            font-size: 14px;
            font-weight: 500;
        }

        .chapter-stats {
            font-size: 12px;
            color: #858585;
            margin-top: 4px;
        }

        #content {
            flex: 1;
            overflow-y: auto;
            padding: 30px;
        }

        #loading {
            text-align: center;
            padding: 40px;
            color: #858585;
        }

        .report-header {
            background: #252526;
            padding: 24px;
            border-radius: 6px;
            margin-bottom: 24px;
            border: 1px solid #3e3e42;
        }

        .report-header h1 {
            font-size: 24px;
            margin-bottom: 16px;
            color: #ffffff;
        }

        .report-meta {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 12px;
            font-size: 13px;
        }

        .meta-item {
            color: #cccccc;
        }

        .meta-label {
            color: #858585;
            font-weight: 500;
        }

        .stats-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 16px;
            margin-bottom: 32px;
        }

        .stat-card {
            background: #252526;
            padding: 20px;
            border-radius: 6px;
            border: 1px solid #3e3e42;
        }

        .stat-label {
            font-size: 12px;
            color: #858585;
            text-transform: uppercase;
            font-weight: 600;
            letter-spacing: 0.5px;
            margin-bottom: 8px;
        }

        .stat-value {
            font-size: 28px;
            font-weight: 600;
            color: #ffffff;
        }

        .stat-detail {
            font-size: 13px;
            color: #cccccc;
            margin-top: 8px;
        }

        .section-title {
            font-size: 18px;
            font-weight: 600;
            margin: 32px 0 16px;
            color: #ffffff;
            border-bottom: 2px solid #3e3e42;
            padding-bottom: 8px;
        }

        .sentence-item {
            background: #252526;
            padding: 20px;
            border-radius: 6px;
            margin-bottom: 16px;
            border-left: 4px solid #3e3e42;
        }

        .sentence-item.unreliable {
            border-left-color: #f48771;
        }

        .sentence-item.attention {
            border-left-color: #dcdcaa;
        }

        .sentence-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 12px;
            flex-wrap: wrap;
            gap: 8px;
        }

        .sentence-id {
            font-size: 14px;
            font-weight: 600;
            color: #4ec9b0;
        }

        .sentence-metrics {
            display: flex;
            gap: 16px;
            font-size: 13px;
        }

        .metric {
            display: flex;
            align-items: center;
            gap: 6px;
        }

        .metric-label {
            color: #858585;
        }

        .metric-value {
            font-weight: 600;
        }

        .metric-value.high {
            color: #f48771;
        }

        .metric-value.medium {
            color: #dcdcaa;
        }

        .metric-value.low {
            color: #4ec9b0;
        }

        .status-badge {
            padding: 4px 10px;
            border-radius: 4px;
            font-size: 11px;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }

        .status-badge.unreliable {
            background: #f487711a;
            color: #f48771;
        }

        .status-badge.attention {
            background: #dcdcaa1a;
            color: #dcdcaa;
        }

        .sentence-details {
            display: grid;
            gap: 8px;
            font-size: 13px;
            color: #cccccc;
            margin-bottom: 12px;
        }

        .sentence-text {
            background: #1e1e1e;
            padding: 12px;
            border-radius: 4px;
            font-family: 'Consolas', 'Courier New', monospace;
            line-height: 1.6;
            margin-top: 8px;
        }

        .text-label {
            font-size: 11px;
            color: #858585;
            text-transform: uppercase;
            font-weight: 600;
            margin-bottom: 4px;
        }

        .text-content {
            color: #d4d4d4;
        }

        .audio-player {
            margin-top: 12px;
            padding: 12px;
            background: #1e1e1e;
            border-radius: 4px;
            border: 1px solid #3e3e42;
        }

        .audio-player audio {
            width: 100%;
            height: 32px;
        }

        .play-button {
            display: inline-flex;
            align-items: center;
            gap: 6px;
            padding: 6px 12px;
            background: #0e639c;
            color: #ffffff;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            font-size: 12px;
            font-weight: 500;
            transition: background 0.2s;
        }

        .play-button:hover {
            background: #1177bb;
        }

        .play-button:active {
            background: #0d5a8f;
        }

        .play-icon {
            width: 0;
            height: 0;
            border-left: 8px solid currentColor;
            border-top: 5px solid transparent;
            border-bottom: 5px solid transparent;
        }

        .paragraph-item {
            background: #252526;
            padding: 18px;
            border-radius: 6px;
            margin-bottom: 12px;
            border-left: 3px solid #3e3e42;
        }

        .paragraph-item.unreliable {
            border-left-color: #f48771;
        }

        .paragraph-item.attention {
            border-left-color: #dcdcaa;
        }

        ::-webkit-scrollbar {
            width: 10px;
            height: 10px;
        }

        ::-webkit-scrollbar-track {
            background: #1e1e1e;
        }

        ::-webkit-scrollbar-thumb {
            background: #424242;
            border-radius: 5px;
        }

        ::-webkit-scrollbar-thumb:hover {
            background: #4e4e4e;
        }
    </style>
</head>
<body>
    <div id="sidebar">
        <h2>Validation Reports</h2>
        <div id="chapter-list"></div>
    </div>
    <div id="content">
        <div id="loading">Select a chapter to view its validation report</div>
    </div>

    <script>
        let chapters = [];
        let currentChapter = null;

        async function loadChapters() {
            try {
                const response = await fetch('/api/chapters');
                chapters = await response.json();
                renderChapterList();
            } catch (error) {
                console.error('Failed to load chapters:', error);
            }
        }

        function renderChapterList() {
            const list = document.getElementById('chapter-list');
            list.innerHTML = chapters.map(chapter => `
                <div class="chapter-item" onclick="loadReport('${chapter.name}')">
                    <div class="chapter-name">${chapter.name}</div>
                    <div class="chapter-stats">${chapter.path}</div>
                </div>
            `).join('');
        }

        async function loadReport(chapterName) {
            currentChapter = chapterName;

            // Update active state
            document.querySelectorAll('.chapter-item').forEach((item, index) => {
                if (chapters[index].name === chapterName) {
                    item.classList.add('active');
                } else {
                    item.classList.remove('active');
                }
            });

            try {
                const response = await fetch(`/api/report/${encodeURIComponent(chapterName)}`);
                const report = await response.json();
                renderReport(report);
            } catch (error) {
                console.error('Failed to load report:', error);
                document.getElementById('content').innerHTML = '<div id="loading">Failed to load report</div>';
            }
        }

        function renderReport(report) {
            const content = document.getElementById('content');

            const sentencesHtml = report.sentences.map(s => `
                <div class="sentence-item ${s.status}">
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
                    </div>
                    <div class="sentence-text">
                        <div class="text-label">Book</div>
                        <div class="text-content">${escapeHtml(s.bookText)}</div>
                        ${s.scriptText ? `
                            <div class="text-label" style="margin-top: 12px;">Script</div>
                            <div class="text-content">${escapeHtml(s.scriptText)}</div>
                        ` : ''}
                    </div>
                    ${s.startTime !== null && s.endTime !== null ? `
                        <div class="audio-player">
                            <button class="play-button" onclick="playAudioSegment('${currentChapter}', ${s.startTime}, ${s.endTime})">
                                <span class="play-icon"></span>
                                Play Audio Segment
                            </button>
                        </div>
                    ` : ''}
                </div>
            `).join('');

            const paragraphsHtml = report.paragraphs.map(p => `
                <div class="paragraph-item ${p.status}">
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
                    </div>
                    ${p.bookText ? `
                        <div class="sentence-text">
                            <div class="text-label">Book</div>
                            <div class="text-content">${escapeHtml(p.bookText)}</div>
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

                <div class="stats-grid">
                    <div class="stat-card">
                        <div class="stat-label">Sentences</div>
                        <div class="stat-value">${report.stats.sentenceCount}</div>
                        <div class="stat-detail">
                            Avg WER ${report.stats.avgWer} | Max WER ${report.stats.maxWer}
                        </div>
                        <div class="stat-detail">Flagged: ${report.stats.flaggedCount}</div>
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

                <div class="section-title">Sentences by WER</div>
                ${sentencesHtml}

                <div class="section-title">Paragraphs by WER</div>
                ${paragraphsHtml}
            `;
        }

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

        function playAudioSegment(chapterName, startTime, endTime) {
            // Create or reuse audio element
            let audioPlayer = document.getElementById('global-audio-player');
            if (!audioPlayer) {
                audioPlayer = document.createElement('audio');
                audioPlayer.id = 'global-audio-player';
                audioPlayer.controls = true;
                audioPlayer.style.display = 'none';
                document.body.appendChild(audioPlayer);
            }

            // Set source with timing parameters
            const url = `/api/audio/${encodeURIComponent(chapterName)}?start=${startTime}&end=${endTime}`;
            audioPlayer.src = url;
            audioPlayer.currentTime = 0;
            audioPlayer.play().catch(err => {
                console.error('Failed to play audio:', err);
                alert('Failed to play audio segment. Check console for details.');
            });
        }

        // Load chapters on page load
        loadChapters();
    </script>
</body>
</html>"""

        self.send_response(200)
        self.send_header('Content-type', 'text/html')
        self.end_headers()
        self.wfile.write(html.encode())

    def serve_chapters_list(self):
        """Find all validation report files and return chapter list"""
        chapters = []

        for item in BASE_DIR.iterdir():
            if item.is_dir():
                # Look for validation report in this directory
                report_pattern = item.name + ".validate.report.txt"
                report_path = item / report_pattern

                if report_path.exists():
                    chapters.append({
                        'name': item.name,
                        'path': str(item.relative_to(BASE_DIR))
                    })

        def natural_sort_key(chapter):
            name = chapter['name']
            number_groups = [int(match.group()) for match in re.finditer(r"\d+", name)]
            if number_groups:
                # Prefer the first number block (chapters typically lead with it)
                primary = number_groups[0]
                # Secondary key ensures ties keep lexicographic order
                return (0, primary, name.lower())
            return (1, name.lower())

        chapters.sort(key=natural_sort_key)

        self.send_response(200)
        self.send_header('Content-type', 'application/json')
        self.end_headers()
        self.wfile.write(json.dumps(chapters).encode())

    def serve_report(self, chapter_name):
        """Parse and serve a validation report"""
        from urllib.parse import unquote

        # Decode URL-encoded chapter name
        chapter_name = unquote(chapter_name)

        chapter_dir = BASE_DIR / chapter_name
        report_file = chapter_dir / f"{chapter_name}.validate.report.txt"

        print(f"Looking for report: {report_file}")
        print(f"File exists: {report_file.exists()}")

        if not report_file.exists():
            error_msg = json.dumps({
                'error': 'Report not found',
                'chapter': chapter_name,
                'path': str(report_file)
            })
            self.send_response(404)
            self.send_header('Content-type', 'application/json')
            self.end_headers()
            self.wfile.write(error_msg.encode())
            return

        try:
            report_data = self.parse_report(report_file, chapter_name)
            self.send_response(200)
            self.send_header('Content-type', 'application/json')
            self.end_headers()
            self.wfile.write(json.dumps(report_data).encode())
        except Exception as e:
            import traceback
            print(f"Error parsing report: {e}")
            print(traceback.format_exc())

            error_msg = json.dumps({
                'error': str(e),
                'traceback': traceback.format_exc()
            })
            self.send_response(500)
            self.send_header('Content-type', 'application/json')
            self.end_headers()
            self.wfile.write(error_msg.encode())

    def parse_report(self, report_path, chapter_name):
        """Parse validation report text file into structured data"""
        with open(report_path, 'r', encoding='utf-8-sig') as f:
            content = f.read()

        lines = content.split('\n')

        # Parse header
        report_data = {
            'chapterName': chapter_name,
            'audioPath': '',
            'scriptPath': '',
            'bookIndex': '',
            'created': '',
            'stats': {},
            'sentences': [],
            'paragraphs': []
        }

        # Extract metadata from header
        for line in lines[:10]:
            if line.startswith('Audio'):
                report_data['audioPath'] = line.split(':', 1)[1].strip()
            elif line.startswith('Script'):
                report_data['scriptPath'] = line.split(':', 1)[1].strip()
            elif line.startswith('Book Index'):
                report_data['bookIndex'] = line.split(':', 1)[1].strip()
            elif line.startswith('Created'):
                report_data['created'] = line.split(':', 1)[1].strip()
            elif line.startswith('Sentences'):
                # Parse: "Sentences : 277 (Avg WER 2.30%, Max WER 100.00%, Flagged 19)"
                match = re.search(r'(\d+)\s*\(Avg WER ([\d.]+)%, Max WER ([\d.]+)%, Flagged (\d+)\)', line)
                if match:
                    report_data['stats']['sentenceCount'] = match.group(1)
                    report_data['stats']['avgWer'] = match.group(2) + '%'
                    report_data['stats']['maxWer'] = match.group(3) + '%'
                    report_data['stats']['flaggedCount'] = match.group(4)
            elif line.startswith('Paragraphs'):
                # Parse: "Paragraphs: 72 (Avg WER 3.53%, Avg Coverage 98.93%)"
                match = re.search(r'(\d+)\s*\(Avg WER ([\d.]+)%, Avg Coverage ([\d.]+)%\)', line)
                if match:
                    report_data['stats']['paragraphCount'] = match.group(1)
                    report_data['stats']['paragraphAvgWer'] = match.group(2) + '%'
                    report_data['stats']['avgCoverage'] = match.group(3) + '%'

        # Parse sentences section
        current_section = None
        current_item = None

        for i, line in enumerate(lines):
            line = line.strip()

            if line == 'All sentences by WER:':
                current_section = 'sentences'
                continue
            elif line == 'All paragraphs by WER:':
                current_section = 'paragraphs'
                continue

            if current_section == 'sentences':
                # Match sentence header: "  #43 | WER 100.0% | CER 100.0% | Status unreliable"
                match = re.match(r'#(\d+)\s*\|\s*WER\s*([\d.]+)%\s*\|\s*CER\s*([\d.]+)%\s*\|\s*Status\s*(\w+)', line)
                if match:
                    if current_item:
                        report_data['sentences'].append(current_item)

                    current_item = {
                        'id': match.group(1),
                        'wer': match.group(2) + '%',
                        'cer': match.group(3) + '%',
                        'status': match.group(4),
                        'bookRange': '',
                        'scriptRange': '',
                        'timing': '',
                        'bookText': '',
                        'scriptText': '',
                        'startTime': None,
                        'endTime': None
                    }
                elif current_item:
                    if line.startswith('Book range:'):
                        current_item['bookRange'] = line.split(':', 1)[1].strip()
                    elif line.startswith('Script range:'):
                        current_item['scriptRange'] = line.split(':', 1)[1].strip()
                    elif line.startswith('Timing:'):
                        timing_str = line.split(':', 1)[1].strip()
                        current_item['timing'] = timing_str
                        # Parse timing: "870.530s → 871.050s (Δ 0.520s)"
                        timing_match = re.search(r'([\d.]+)s\s*→\s*([\d.]+)s', timing_str)
                        if timing_match:
                            current_item['startTime'] = float(timing_match.group(1))
                            current_item['endTime'] = float(timing_match.group(2))
                    elif line.startswith('Book   :'):
                        current_item['bookText'] = line.split(':', 1)[1].strip()
                    elif line.startswith('Script :'):
                        current_item['scriptText'] = line.split(':', 1)[1].strip()

            elif current_section == 'paragraphs':
                # Match paragraph header: "  #44 | WER 100.0% | Coverage 100.0% | Status unreliable"
                match = re.match(r'#(\d+)\s*\|\s*WER\s*([\d.]+)%\s*\|\s*Coverage\s*([\d.]+)%\s*\|\s*Status\s*(\w+)', line)
                if match:
                    if current_item and 'coverage' in current_item:
                        report_data['paragraphs'].append(current_item)

                    current_item = {
                        'id': match.group(1),
                        'wer': match.group(2) + '%',
                        'coverage': match.group(3) + '%',
                        'status': match.group(4),
                        'bookRange': '',
                        'bookText': ''
                    }
                elif current_item and 'coverage' in current_item:
                    if line.startswith('Book range:'):
                        current_item['bookRange'] = line.split(':', 1)[1].strip()
                    elif line.startswith('Book   :'):
                        current_item['bookText'] = line.split(':', 1)[1].strip()

        # Add last item
        if current_item:
            if current_section == 'sentences':
                report_data['sentences'].append(current_item)
            elif current_section == 'paragraphs' and 'coverage' in current_item:
                report_data['paragraphs'].append(current_item)

        return report_data

    def serve_audio_segment(self, chapter_name, query_string):
        """Serve an audio segment for a specific chapter and time range"""
        from urllib.parse import unquote, parse_qs
        import subprocess
        import tempfile

        # Parse chapter name and query parameters
        chapter_name = unquote(chapter_name)
        query_params = parse_qs(query_string) if query_string else {}

        start_time = float(query_params.get('start', [0])[0])
        end_time = float(query_params.get('end', [0])[0])

        # Find the audio file - it should be at the book root with the same name as the chapter folder
        audio_path = BASE_DIR / f"{chapter_name}.wav"

        print(f"Looking for audio: {audio_path}")
        print(f"Audio exists: {audio_path.exists()}")
        print(f"Segment: {start_time}s to {end_time}s")

        if not audio_path.exists():
            error_msg = json.dumps({
                'error': 'Audio file not found',
                'path': str(audio_path)
            })
            self.send_response(404)
            self.send_header('Content-type', 'application/json')
            self.end_headers()
            self.wfile.write(error_msg.encode())
            return

        try:
            # Use ffmpeg to extract the audio segment
            duration = end_time - start_time
            with tempfile.NamedTemporaryFile(suffix='.wav', delete=False) as temp_file:
                temp_path = temp_file.name

            # Extract segment using ffmpeg
            cmd = [
                'ffmpeg',
                '-i', str(audio_path),
                '-ss', str(start_time),
                '-t', str(duration),
                '-c', 'copy',
                '-y',
                temp_path
            ]

            result = subprocess.run(cmd, capture_output=True, text=True)

            if result.returncode != 0:
                raise Exception(f"ffmpeg failed: {result.stderr}")

            # Serve the file
            with open(temp_path, 'rb') as f:
                audio_data = f.read()

            self.send_response(200)
            self.send_header('Content-type', 'audio/wav')
            self.send_header('Content-Length', str(len(audio_data)))
            self.end_headers()
            self.wfile.write(audio_data)

            # Clean up temp file
            os.unlink(temp_path)

        except Exception as e:
            import traceback
            print(f"Error serving audio: {e}")
            print(traceback.format_exc())

            error_msg = json.dumps({
                'error': str(e),
                'traceback': traceback.format_exc()
            })
            self.send_response(500)
            self.send_header('Content-type', 'application/json')
            self.end_headers()
            self.wfile.write(error_msg.encode())

    def log_message(self, format, *args):
        """Custom log format"""
        print(f"[{self.log_date_time_string()}] {format % args}")


def main():
    global BASE_DIR, PORT

    # Parse command-line arguments
    if len(sys.argv) > 1:
        BASE_DIR = Path(sys.argv[1])
    if len(sys.argv) > 2:
        PORT = int(sys.argv[2])

    if not BASE_DIR.exists():
        print(f"Error: Base directory does not exist: {BASE_DIR}")
        sys.exit(1)

    server_address = ('', PORT)
    httpd = HTTPServer(server_address, ValidationReportHandler)

    print(f"="*60)
    print(f"Validation Report Viewer")
    print(f"="*60)
    print(f"Serving reports from: {BASE_DIR}")
    print(f"Server running at: http://localhost:{PORT}")
    print(f"Press Ctrl+C to stop")
    print(f"="*60)

    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        print("\n\nShutting down server...")
        httpd.shutdown()


if __name__ == '__main__':
    main()

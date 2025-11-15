#!/usr/bin/env python3
"""
Validation Report Viewer Server
Serves validation reports from chapter directories with a web UI
"""

import os
import sys
import json
import re
import shutil
import subprocess
import tempfile
import traceback
import copy
from pathlib import Path
from datetime import datetime, timedelta
from http.server import HTTPServer, BaseHTTPRequestHandler
from urllib.parse import parse_qs, urlparse
from xml.etree import ElementTree as ET
from zipfile import ZipFile
import openpyxl
from openpyxl.utils import get_column_letter
import xlwings as xw

# Get AppData directory for reviewed status
APPDATA_DIR = Path(os.getenv('APPDATA')) / 'AMS' / 'validation-viewer'
APPDATA_DIR.mkdir(parents=True, exist_ok=True)
REVIEWED_STATUS_FILE = APPDATA_DIR / 'reviewed-status.json'

# Default configuration
BASE_DIR = Path(r"C:\Aethon\InProgress\Vain Glory 2")
PORT = 8081

SCRIPT_DIR = Path(__file__).resolve().parent
REPO_ROOT = SCRIPT_DIR.parent.parent
CRX_TEMPLATE_PATH = Path(r"C:\Aethon\BASE_CRX.xlsx")
CRX_DIR_NAME = 'CRX'
CRX_DATA_ROW_START = 11
DEFAULT_ERROR_TYPE = 'MR'

EXCEL_NS = 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'
XML_NS = 'http://www.w3.org/XML/1998/namespace'
REL_NS = 'http://schemas.openxmlformats.org/officeDocument/2006/relationships'
MC_NS = 'http://schemas.openxmlformats.org/markup-compatibility/2006'
X14AC_NS = 'http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac'
XR_NS = 'http://schemas.microsoft.com/office/spreadsheetml/2014/revision'
XR2_NS = 'http://schemas.openxmlformats.org/spreadsheetml/2015/revision2'
XR3_NS = 'http://schemas.microsoft.com/office/spreadsheetml/2016/revision3'

NAMESPACES = {'a': EXCEL_NS}
CELL_REF_RE = re.compile(r'([A-Z]+)(\d+)')

for prefix, uri in [('', EXCEL_NS), ('r', REL_NS), ('mc', MC_NS), ('x14ac', X14AC_NS), ('xr', XR_NS), ('xr2', XR2_NS), ('xr3', XR3_NS)]:
    ET.register_namespace(prefix, uri)


class ValidationReportHandler(BaseHTTPRequestHandler):
    @staticmethod
    def load_reviewed_status():
        """Load reviewed status from AppData"""
        if REVIEWED_STATUS_FILE.exists():
            try:
                with open(REVIEWED_STATUS_FILE, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                    # Key by book name
                    book_name = BASE_DIR.name
                    return data.get(book_name, {})
            except Exception as e:
                print(f"Error loading reviewed status: {e}")
        return {}

    @staticmethod
    def save_reviewed_status(reviewed_chapters):
        """Save reviewed status to AppData"""
        try:
            # Load existing data
            all_data = {}
            if REVIEWED_STATUS_FILE.exists():
                with open(REVIEWED_STATUS_FILE, 'r', encoding='utf-8') as f:
                    all_data = json.load(f)

            # Update for current book
            book_name = BASE_DIR.name
            all_data[book_name] = reviewed_chapters

            # Save back
            with open(REVIEWED_STATUS_FILE, 'w', encoding='utf-8') as f:
                json.dump(all_data, f, indent=2)
        except Exception as e:
            print(f"Error saving reviewed status: {e}")

    def do_GET(self):
        parsed_path = urlparse(self.path)
        path = parsed_path.path

        if path == '/':
            self.serve_index()
        elif path == '/api/chapters':
            self.serve_chapters_list()
        elif path == '/api/overview':
            self.serve_overview()
        elif path == '/api/reviewed':
            self.serve_reviewed_status()
        elif path.startswith('/api/report/'):
            chapter_name = path[len('/api/report/'):]
            self.serve_report(chapter_name)
        elif path.startswith('/api/audio/'):
            # Extract chapter name and timing from path
            remainder = path[len('/api/audio/'):]
            self.serve_audio_segment(remainder, parsed_path.query)
        elif path.startswith('/static/'):
            self.serve_static_file(path[8:])  # Remove '/static/' prefix
        else:
            self.send_error(404)

    def do_POST(self):
        parsed_path = urlparse(self.path)
        path = parsed_path.path
        print(f"POST request path: {path}")

        if path.startswith('/api/export/'):
            chapter_name = path[len('/api/export/'):]
            print(f"Matched export route, chapter: {chapter_name}")
            self.handle_export_audio(chapter_name)
        elif path.startswith('/api/crx/'):
            chapter_name = path[len('/api/crx/'):]
            print(f"Matched CRX route, chapter: {chapter_name}")
            self.handle_add_to_crx(chapter_name)
        elif path.startswith('/api/reviewed/'):
            chapter_name = path[len('/api/reviewed/'):]
            print(f"Matched reviewed route, chapter: {chapter_name}")
            self.handle_mark_reviewed(chapter_name)
        elif path == '/api/reset-reviews':
            print(f"Matched reset reviews route")
            self.handle_reset_reviews()
        else:
            print(f"No route matched for path: {path}")
            self.send_error(404)

    def serve_reviewed_status(self):
        """Serve reviewed status for all chapters"""
        reviewed = self.load_reviewed_status()
        self.send_json_response(reviewed)

    def handle_mark_reviewed(self, chapter_name):
        """Mark a chapter as reviewed"""
        from urllib.parse import unquote

        chapter_name = unquote(chapter_name)

        # Read POST data
        content_length = int(self.headers['Content-Length'])
        post_data = self.rfile.read(content_length)
        params = json.loads(post_data.decode('utf-8'))

        reviewed_status = params.get('reviewed', True)

        try:
            # Load current status
            all_reviewed = self.load_reviewed_status()

            # Update status for this chapter
            all_reviewed[chapter_name] = {
                'reviewed': reviewed_status,
                'timestamp': datetime.utcnow().isoformat()
            }

            # Save back
            self.save_reviewed_status(all_reviewed)

            self.send_json_response({
                'success': True,
                'chapter': chapter_name,
                'reviewed': reviewed_status
            })

        except Exception as e:
            print(f"Error marking reviewed: {e}")
            print(traceback.format_exc())
            self.send_json_response({'error': str(e)}, 500)

    def handle_reset_reviews(self):
        """Reset all review status for the current book"""
        try:
            # Clear all reviewed status for current book
            self.save_reviewed_status({})

            self.send_json_response({
                'success': True,
                'message': 'All review status reset'
            })

        except Exception as e:
            print(f"Error resetting reviews: {e}")
            print(traceback.format_exc())
            self.send_json_response({'error': str(e)}, 500)

    def serve_static_file(self, file_path):
        """Serve static files (CSS, JS)"""
        static_dir = SCRIPT_DIR / 'static'
        full_path = static_dir / file_path

        if not full_path.exists() or not full_path.is_file():
            self.send_error(404)
            return

        # Determine content type
        content_type = 'text/plain'
        if file_path.endswith('.css'):
            content_type = 'text/css'
        elif file_path.endswith('.js'):
            content_type = 'application/javascript'

        try:
            with open(full_path, 'r', encoding='utf-8') as f:
                content = f.read()

            self.send_response(200)
            self.send_header('Content-type', content_type)
            self.send_header('Content-Length', str(len(content.encode())))
            self.end_headers()
            self.wfile.write(content.encode())
        except Exception as e:
            print(f"Error serving static file {file_path}: {e}")
            self.send_error(500)

    def serve_index(self):
        """Serve the main HTML page"""
        html = """<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Validation Report Viewer</title>
    <link rel="stylesheet" href="/static/styles.css">
</head>
<body>
    <div id="toast-container" class="toast-container"></div>
    <div id="sidebar">
        <h2>Validation Reports</h2>
        <div id="chapter-list"></div>
    </div>
    <div id="content">
        <div id="loading">Select a chapter to view its validation report</div>
    </div>

    <!-- CRX Modal -->
    <div id="crxModal" class="modal">
        <div class="modal-content" style="max-width: 700px;">
            <div class="modal-header">Add to CRX</div>
            <div class="modal-body">
                <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px;">
                    <div>
                        <label for="errorType">Error Type:</label>
                        <select id="errorType" class="modal-input">
                            <option value="MR" selected>MR - Misread</option>
                            <option value="PRON">PRON - Pronunciation</option>
                            <option value="DIC">DIC - Diction</option>
                            <option value="NZ">NZ - Noise</option>
                            <option value="PL">PL - Plosive</option>
                            <option value="DIST">DIST - Distortion</option>
                            <option value="MW">MW - Missing Word</option>
                            <option value="ML">ML - Missing Line</option>
                            <option value="TYPO">TYPO - Possible Typo</option>
                            <option value="CHAR">CHAR - Character Voice</option>
                        </select>

                        <label style="margin-top: 12px; display: block;">Errors to Highlight:</label>
                        <div style="background: #1e1e1e; padding: 8px; border-radius: 4px; margin-top: 4px;">
                            <label style="display: block; margin-bottom: 4px; font-size: 12px;">
                                <input type="checkbox" id="selectAllErrors" onchange="toggleAllErrors(this.checked)">
                                <strong>Select All</strong>
                            </label>
                            <div id="errorCheckboxes" style="margin-left: 16px; font-size: 12px;"></div>
                        </div>

                        <label for="paddingSlider" style="margin-top: 12px; display: block;">
                            Tail Padding: <span id="paddingValue">50</span>ms
                        </label>
                        <input type="range" id="paddingSlider" min="0" max="200" value="50" step="10"
                               class="modal-slider" oninput="updatePaddingValue(this.value)">
                    </div>

                    <div>
                        <div class="audio-preview-container">
                            <div class="audio-preview-label">Audio Preview</div>
                            <audio id="crxAudioPreview" controls>
                                <source id="crxAudioSource" type="audio/wav">
                            </audio>
                            <button class="refresh-preview-button" onclick="refreshAudioPreview()">
                                Refresh Preview
                            </button>
                        </div>
                    </div>
                </div>

                <label for="comments" style="margin-top: 16px; display: block;">Comments:</label>
                <textarea id="comments" class="modal-textarea" placeholder="Auto-generated from diff..." rows="4"></textarea>
            </div>
            <div class="modal-footer">
                <button class="modal-button modal-button-secondary" onclick="closeCrxModal()">Cancel</button>
                <button class="modal-button modal-button-primary" onclick="submitCrx()">Add to CRX</button>
            </div>
        </div>
    </div>

    <script src="/static/app.js"></script>
</body>
</html>"""

        self.send_response(200)
        self.send_header('Content-type', 'text/html')
        self.end_headers()
        self.wfile.write(html.encode())

    def extract_hydrate_metrics(self, hydrate_path):
        """Extract summary metrics from hydrate.json"""
        try:
            with open(hydrate_path, 'r', encoding='utf-8') as f:
                hydrate_data = json.load(f)

            sentences = hydrate_data.get('sentences', [])
            paragraphs = hydrate_data.get('paragraphs', [])

            # Count flagged sentences
            flagged_sentences = [s for s in sentences if s.get('status', 'ok') != 'ok']

            # Calculate average WER
            total_wer = sum(s.get('metrics', {}).get('wer', 0) for s in sentences if 'metrics' in s)
            avg_wer = (total_wer / len(sentences) * 100) if sentences else 0

            return {
                'sentenceCount': len(sentences),
                'sentenceFlagged': len(flagged_sentences),
                'sentenceAvgWer': f"{avg_wer:.2f}%",
                'paragraphCount': len(paragraphs),
                'paragraphFlagged': 0,
                'paragraphAvgWer': '0.00%'
            }
        except Exception as e:
            print(f"Error extracting metrics from {hydrate_path}: {e}")
            return {
                'sentenceCount': 0,
                'sentenceFlagged': 0,
                'sentenceAvgWer': 'N/A',
                'paragraphCount': 0,
                'paragraphFlagged': 0,
                'paragraphAvgWer': 'N/A'
            }

    def extract_report_metrics(self, report_path):
        """Extract summary metrics from a validation report"""
        try:
            with open(report_path, 'r', encoding='utf-8-sig') as f:
                content = f.read()

            metrics = {
                'sentenceCount': 0,
                'sentenceFlagged': 0,
                'sentenceAvgWer': '0.00%',
                'paragraphCount': 0,
                'paragraphFlagged': 0,
                'paragraphAvgWer': '0.00%'
            }

            # Parse sentences line: "Sentences : 277 (Avg WER 2.48%, Max WER 100.00%, Flagged 18)"
            sent_match = re.search(r'Sentences\s*:\s*(\d+)\s*\(Avg WER ([\d.]+)%.*Flagged (\d+)\)', content)
            if sent_match:
                metrics['sentenceCount'] = int(sent_match.group(1))
                metrics['sentenceAvgWer'] = sent_match.group(2) + '%'
                metrics['sentenceFlagged'] = int(sent_match.group(3))

            # Parse paragraphs line: "Paragraphs: 72 (Avg WER 3.57%, Avg Coverage 99.89%)"
            para_match = re.search(r'Paragraphs\s*:\s*(\d+)\s*\(Avg WER ([\d.]+)%', content)
            if para_match:
                metrics['paragraphCount'] = int(para_match.group(1))
                metrics['paragraphAvgWer'] = para_match.group(2) + '%'

            # Count flagged paragraphs
            para_section = content.split('All paragraphs by WER:')
            if len(para_section) > 1:
                flagged_paras = re.findall(r'#\d+ \| WER [\d.]+% \| Coverage [\d.]+% \| Status (attention|unreliable)', para_section[1])
                metrics['paragraphFlagged'] = len(flagged_paras)

            return metrics
        except Exception as e:
            print(f"Error extracting metrics from {report_path}: {e}")
            return {
                'sentenceCount': 0,
                'sentenceFlagged': 0,
                'sentenceAvgWer': 'N/A',
                'paragraphCount': 0,
                'paragraphFlagged': 0,
                'paragraphAvgWer': 'N/A'
            }

    def serve_chapters_list(self):
        """Find all hydrate files and return chapter list with metrics"""
        chapters = []

        for item in BASE_DIR.iterdir():
            if item.is_dir():
                # Look for hydrate.json in this directory
                hydrate_pattern = item.name + ".align.hydrate.json"
                hydrate_path = item / hydrate_pattern

                if hydrate_path.exists():
                    # Extract basic metrics from hydrate
                    metrics = self.extract_hydrate_metrics(hydrate_path)
                    chapters.append({
                        'name': item.name,
                        'path': str(item.relative_to(BASE_DIR)),
                        'metrics': metrics
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

    def serve_overview(self):
        """Serve book-wide overview with aggregated metrics"""
        chapters = []

        for item in BASE_DIR.iterdir():
            if item.is_dir():
                hydrate_pattern = item.name + ".align.hydrate.json"
                hydrate_path = item / hydrate_pattern

                if hydrate_path.exists():
                    metrics = self.extract_hydrate_metrics(hydrate_path)
                    chapters.append({
                        'name': item.name,
                        'path': str(item.relative_to(BASE_DIR)),
                        'metrics': metrics
                    })

        def natural_sort_key(chapter):
            name = chapter['name']
            number_groups = [int(match.group()) for match in re.finditer(r"\d+", name)]
            if number_groups:
                primary = number_groups[0]
                return (0, primary, name.lower())
            return (1, name.lower())

        chapters.sort(key=natural_sort_key)

        # Calculate book-wide totals
        total_sentences = sum(c['metrics']['sentenceCount'] for c in chapters)
        total_flagged_sentences = sum(c['metrics']['sentenceFlagged'] for c in chapters)
        total_paragraphs = sum(c['metrics']['paragraphCount'] for c in chapters)
        total_flagged_paragraphs = sum(c['metrics']['paragraphFlagged'] for c in chapters)

        # Calculate weighted average WER
        if total_sentences > 0:
            weighted_sent_wer_sum = sum(
                c['metrics']['sentenceCount'] * float(c['metrics']['sentenceAvgWer'].rstrip('%'))
                for c in chapters if c['metrics']['sentenceAvgWer'] != 'N/A'
            )
            avg_sentence_wer = weighted_sent_wer_sum / total_sentences
        else:
            avg_sentence_wer = 0

        if total_paragraphs > 0:
            weighted_para_wer_sum = sum(
                c['metrics']['paragraphCount'] * float(c['metrics']['paragraphAvgWer'].rstrip('%'))
                for c in chapters if c['metrics']['paragraphAvgWer'] != 'N/A'
            )
            avg_paragraph_wer = weighted_para_wer_sum / total_paragraphs
        else:
            avg_paragraph_wer = 0

        overview = {
            'bookName': BASE_DIR.name,
            'chapterCount': len(chapters),
            'totalSentences': total_sentences,
            'totalFlaggedSentences': total_flagged_sentences,
            'avgSentenceWer': f"{avg_sentence_wer:.2f}%",
            'totalParagraphs': total_paragraphs,
            'totalFlaggedParagraphs': total_flagged_paragraphs,
            'avgParagraphWer': f"{avg_paragraph_wer:.2f}%",
            'chapters': chapters
        }

        self.send_response(200)
        self.send_header('Content-type', 'application/json')
        self.end_headers()
        self.wfile.write(json.dumps(overview).encode())

    def serve_report(self, chapter_name):
        """Parse and serve a validation report from hydrate.json"""
        from urllib.parse import unquote

        # Decode URL-encoded chapter name
        chapter_name = unquote(chapter_name)

        chapter_dir = BASE_DIR / chapter_name
        hydrate_file = chapter_dir / f"{chapter_name}.align.hydrate.json"

        print(f"Looking for hydrate: {hydrate_file}")
        print(f"Hydrate exists: {hydrate_file.exists()}")

        if not hydrate_file.exists():
            error_msg = json.dumps({
                'error': 'Hydrate file not found',
                'chapter': chapter_name,
                'path': str(hydrate_file)
            })
            self.send_response(404)
            self.send_header('Content-type', 'application/json')
            self.end_headers()
            self.wfile.write(error_msg.encode())
            return

        try:
            # Load hydrate data
            with open(hydrate_file, 'r', encoding='utf-8') as f:
                hydrate_data = json.load(f)

            report_data = self.parse_hydrate_to_report(hydrate_data, chapter_name)
            self.send_response(200)
            self.send_header('Content-type', 'application/json')
            self.end_headers()
            self.wfile.write(json.dumps(report_data).encode())
        except Exception as e:
            import traceback
            print(f"Error parsing hydrate: {e}")
            print(traceback.format_exc())

            error_msg = json.dumps({
                'error': str(e),
                'traceback': traceback.format_exc()
            })
            self.send_response(500)
            self.send_header('Content-type', 'application/json')
            self.end_headers()
            self.wfile.write(error_msg.encode())

    def parse_hydrate_to_report(self, hydrate_data, chapter_name):
        """Convert hydrate.json data to report format expected by the UI"""
        sentences = hydrate_data.get('sentences', [])
        paragraphs = hydrate_data.get('paragraphs', [])

        # Calculate statistics
        flagged_sentences = [s for s in sentences if s.get('status', 'ok') != 'ok']
        total_wer = sum(s.get('metrics', {}).get('wer', 0) for s in sentences if 'metrics' in s)
        avg_wer = (total_wer / len(sentences) * 100) if sentences else 0
        max_wer = max((s.get('metrics', {}).get('wer', 0) for s in sentences if 'metrics' in s), default=0) * 100

        # Convert sentences to UI format
        ui_sentences = []
        for sent in sentences:
            metrics = sent.get('metrics', {})
            timing = sent.get('timing', {})
            book_range = sent.get('bookRange', {})
            script_range = sent.get('scriptRange', {})

            wer = metrics.get('wer', 0) * 100
            cer = metrics.get('cer', 0) * 100

            ui_sent = {
                'id': sent.get('id'),
                'wer': f"{wer:.1f}%",
                'cer': f"{cer:.1f}%",
                'status': sent.get('status', 'ok'),
                'bookRange': f"{book_range.get('start', 0)}-{book_range.get('end', 0)}",
                'scriptRange': f"{script_range.get('start', 0)}-{script_range.get('end', 0)}",
                'timing': f"{timing.get('startSec', 0):.3f}s → {timing.get('endSec', 0):.3f}s (Δ {timing.get('duration', 0):.3f}s)",
                'bookText': sent.get('bookText', ''),
                'scriptText': sent.get('scriptText', ''),
                'excerpt': sent.get('bookText', '')[:100],
                'diff': sent.get('diff', {}),  # Include the full diff structure
                'startTime': timing.get('startSec', 0),
                'endTime': timing.get('endSec', 0),
                'bookRangeStart': book_range.get('start'),
                'bookRangeEnd': book_range.get('end')
            }
            ui_sentences.append(ui_sent)

        # Don't sort here - let frontend decide ordering based on view type
        # ui_sentences are already in chronological order (ID order) from hydrate.json

        # Build report data structure
        report_data = {
            'chapterName': chapter_name,
            'audioPath': hydrate_data.get('audioPath', ''),
            'scriptPath': '',
            'bookIndex': '',
            'created': datetime.now().isoformat(),
            'stats': {
                'sentenceCount': str(len(sentences)),
                'avgWer': f"{avg_wer:.2f}%",
                'maxWer': f"{max_wer:.2f}%",
                'flaggedCount': str(len(flagged_sentences)),
                'paragraphCount': str(len(paragraphs)),
                'paragraphAvgWer': '0.00%',
                'avgCoverage': '100.00%'
            },
            'sentences': ui_sentences,
            'paragraphs': []
        }

        return report_data

    def parse_report(self, report_path, chapter_name, hydrate_data=None):
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

        def parse_index_range(range_text):
            if not range_text:
                return None, None

            match = re.search(r'(\d+)\s*-\s*(\d+)', range_text)
            if match:
                return int(match.group(1)), int(match.group(2))

            match = re.search(r'(\d+)', range_text)
            if match:
                value = int(match.group(1))
                return value, value

            return None, None

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
                        'id': int(match.group(1)),
                        'wer': match.group(2) + '%',
                        'cer': match.group(3) + '%',
                        'status': match.group(4),
                        'bookRange': '',
                        'scriptRange': '',
                        'timing': '',
                        'bookText': '',
                        'scriptText': '',
                        'excerpt': '',
                        'startTime': None,
                        'endTime': None,
                        'bookRangeStart': None,
                        'bookRangeEnd': None
                    }
                elif current_item:
                    if line.startswith('Book range:'):
                        range_text = line.split(':', 1)[1].strip()
                        current_item['bookRange'] = range_text
                        start, end = parse_index_range(range_text)
                        current_item['bookRangeStart'] = start
                        current_item['bookRangeEnd'] = end
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
                    elif line.startswith('Excerpt:'):
                        current_item['excerpt'] = line.split(':', 1)[1].strip()

            elif current_section == 'paragraphs':
                # Match paragraph header: "  #44 | WER 100.0% | Coverage 100.0% | Status unreliable"
                match = re.match(r'#(\d+)\s*\|\s*WER\s*([\d.]+)%\s*\|\s*Coverage\s*([\d.]+)%\s*\|\s*Status\s*(\w+)', line)
                if match:
                    if current_item and 'coverage' in current_item:
                        report_data['paragraphs'].append(current_item)

                    current_item = {
                        'id': int(match.group(1)),
                        'wer': match.group(2) + '%',
                        'coverage': match.group(3) + '%',
                        'status': match.group(4),
                        'bookRange': '',
                        'bookText': '',
                        'bookRangeStart': None,
                        'bookRangeEnd': None,
                        'sentenceIds': [],
                        'startTime': None,
                        'endTime': None,
                        'timing': ''
                    }
                elif current_item and 'coverage' in current_item:
                    if line.startswith('Book range:'):
                        range_text = line.split(':', 1)[1].strip()
                        current_item['bookRange'] = range_text
                        start, end = parse_index_range(range_text)
                        current_item['bookRangeStart'] = start
                        current_item['bookRangeEnd'] = end
                    elif line.startswith('Book   :'):
                        current_item['bookText'] = line.split(':', 1)[1].strip()

        # Add last item
        if current_item:
            if current_section == 'sentences':
                report_data['sentences'].append(current_item)
            elif current_section == 'paragraphs' and 'coverage' in current_item:
                report_data['paragraphs'].append(current_item)

        # Map paragraphs to sentences for linking and audio timing
        # Use hydrate data if available to get complete sentence list with timing
        if hydrate_data and 'paragraphs' in hydrate_data and 'sentences' in hydrate_data:
            # Create lookup for hydrate sentences by ID
            hydrate_sentences = {s['id']: s for s in hydrate_data['sentences']}
            hydrate_paragraphs = {p['id']: p for p in hydrate_data['paragraphs']}

            # Build a map from sentence ID to paragraph ID
            sentence_to_paragraph = {}
            for para in hydrate_data['paragraphs']:
                for sent_id in para.get('sentenceIds', []):
                    sentence_to_paragraph[sent_id] = para['id']

            # Build alignment word operations grouped by sentence, preserving order
            word_ops_by_sentence = {}
            words = hydrate_data.get('words') or []
            if words:
                # Map each book word index to its sentence ID for quick lookup
                book_to_sentence = {}
                for sent in hydrate_data['sentences']:
                    book_range = sent.get('bookRange') or {}
                    start = book_range.get('start')
                    end = book_range.get('end')
                    if start is None or end is None:
                        continue
                    if end < start:
                        start, end = end, start

                    for idx in range(start, end + 1):
                        book_to_sentence[idx] = sent['id']

                    word_ops_by_sentence.setdefault(sent['id'], [])

                last_sentence_id = None
                for word in words:
                    sentence_id = None
                    book_idx = word.get('bookIdx')
                    if book_idx is not None:
                        sentence_id = book_to_sentence.get(book_idx)
                    if sentence_id is None:
                        sentence_id = last_sentence_id
                    if sentence_id is None:
                        continue

                    entry = {
                        'op': word.get('op'),
                        'reason': word.get('reason'),
                        'bookWord': (word.get('bookWord') or '').strip(),
                        'asrWord': (word.get('asrWord') or '').strip()
                    }
                    word_ops_by_sentence.setdefault(sentence_id, []).append(entry)
                    last_sentence_id = sentence_id

            # Add paragraph ID to each sentence
            for sentence in report_data['sentences']:
                sentence['paragraphId'] = sentence_to_paragraph.get(sentence['id'])
                if word_ops_by_sentence:
                    ops = word_ops_by_sentence.get(sentence['id'])
                    if ops:
                        sentence['wordOps'] = ops

            for paragraph in report_data['paragraphs']:
                para_id = paragraph.get('id')
                hydrate_para = hydrate_paragraphs.get(para_id)

                if hydrate_para and 'sentenceIds' in hydrate_para:
                    # Get ALL sentence IDs from hydrate (not just flagged ones)
                    all_sentence_ids = hydrate_para['sentenceIds']
                    paragraph['sentenceIds'] = all_sentence_ids

                    # Find which of these sentences are flagged (in report_data['sentences'])
                    flagged_ids = {s['id'] for s in report_data['sentences']}
                    paragraph['flaggedSentenceIds'] = [sid for sid in all_sentence_ids if sid in flagged_ids]

                    # Calculate timing from ALL sentences in paragraph
                    # Also include last sentence of previous paragraph and first sentence of next paragraph
                    context_sentence_ids = all_sentence_ids.copy()

                    # Add last sentence from previous paragraph (by ID, not by sorted position)
                    prev_para_id = para_id - 1
                    if prev_para_id >= 0:
                        prev_hydrate = hydrate_paragraphs.get(prev_para_id)
                        if prev_hydrate and 'sentenceIds' in prev_hydrate and prev_hydrate['sentenceIds']:
                            last_prev_sent_id = prev_hydrate['sentenceIds'][-1]
                            if last_prev_sent_id not in context_sentence_ids:
                                context_sentence_ids.insert(0, last_prev_sent_id)

                    # Add first sentence from next paragraph (by ID, not by sorted position)
                    next_para_id = para_id + 1
                    next_hydrate = hydrate_paragraphs.get(next_para_id)
                    if next_hydrate and 'sentenceIds' in next_hydrate and next_hydrate['sentenceIds']:
                        first_next_sent_id = next_hydrate['sentenceIds'][0]
                        if first_next_sent_id not in context_sentence_ids:
                            context_sentence_ids.append(first_next_sent_id)

                    start_times = []
                    end_times = []
                    for sent_id in context_sentence_ids:
                        hydrate_sent = hydrate_sentences.get(sent_id)
                        if hydrate_sent and 'timing' in hydrate_sent:
                            timing = hydrate_sent['timing']
                            if 'startSec' in timing and 'endSec' in timing:
                                start_times.append(timing['startSec'])
                                end_times.append(timing['endSec'])

                    if start_times and end_times:
                        paragraph_start = min(start_times)
                        paragraph_end = max(end_times)
                        paragraph['startTime'] = paragraph_start
                        paragraph['endTime'] = paragraph_end
                        duration = paragraph_end - paragraph_start
                        paragraph['timing'] = f"{paragraph_start:.3f}s → {paragraph_end:.3f}s (Δ {duration:.3f}s)"
                    else:
                        paragraph['startTime'] = None
                        paragraph['endTime'] = None
                        paragraph['timing'] = ''
                else:
                    # Fallback if hydrate data not available for this paragraph
                    paragraph['sentenceIds'] = []
                    paragraph['flaggedSentenceIds'] = []
                    paragraph['startTime'] = None
                    paragraph['endTime'] = None
                    paragraph['timing'] = ''
        else:
            # Original fallback logic using only flagged sentences
            for paragraph in report_data['paragraphs']:
                start = paragraph.get('bookRangeStart')
                end = paragraph.get('bookRangeEnd')
                if start is None or end is None:
                    paragraph['sentenceIds'] = []
                    paragraph['flaggedSentenceIds'] = []
                    paragraph['startTime'] = None
                    paragraph['endTime'] = None
                    paragraph['timing'] = ''
                    continue

                sentence_ids = []
                start_times = []
                end_times = []

                for sentence in report_data['sentences']:
                    sentence_start = sentence.get('bookRangeStart')
                    sentence_end = sentence.get('bookRangeEnd')
                    if sentence_start is None or sentence_end is None:
                        continue

                    if sentence_end < start or sentence_start > end:
                        continue

                    sentence_ids.append(sentence['id'])
                    if sentence.get('startTime') is not None:
                        start_times.append(sentence['startTime'])
                    if sentence.get('endTime') is not None:
                        end_times.append(sentence['endTime'])

                sentence_ids.sort()
                paragraph['sentenceIds'] = sentence_ids
                paragraph['flaggedSentenceIds'] = sentence_ids  # All are flagged in fallback mode

                if start_times and end_times:
                    paragraph_start = min(start_times)
                    paragraph_end = max(end_times)
                    if paragraph_end < paragraph_start:
                        paragraph_end = paragraph_start
                    paragraph['startTime'] = paragraph_start
                    paragraph['endTime'] = paragraph_end
                    duration = paragraph_end - paragraph_start
                    paragraph['timing'] = f"{paragraph_start:.3f}s → {paragraph_end:.3f}s (Δ {duration:.3f}s)"
                else:
                    paragraph['startTime'] = None
                    paragraph['endTime'] = None
                    paragraph['timing'] = ''

        return report_data

    def serve_audio_segment(self, chapter_name, query_string):
        """Serve an audio segment for a specific chapter and time range"""
        from urllib.parse import unquote, parse_qs
        import subprocess
        import tempfile

        # Parse chapter name and query parameters
        chapter_name = unquote(chapter_name)
        query_params = parse_qs(query_string) if query_string else {}

        start_time = float(query_params.get('start', [0])[0]) if 'start' in query_params else None
        end_time = float(query_params.get('end', [0])[0]) if 'end' in query_params else None

        # Find the audio file - try chapter folder first, then book root
        audio_path = BASE_DIR / chapter_name / f"{chapter_name}.treated.wav"
        if not audio_path.exists():
            audio_path = BASE_DIR / chapter_name / f"{chapter_name}.wav"
        if not audio_path.exists():
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
            # If no segment specified, serve the whole file
            if start_time is None or end_time is None:
                with open(audio_path, 'rb') as f:
                    audio_data = f.read()

                self.send_response(200)
                self.send_header('Content-type', 'audio/wav')
                self.send_header('Content-Length', str(len(audio_data)))
                self.send_header('Accept-Ranges', 'bytes')
                self.end_headers()
                self.wfile.write(audio_data)
                return

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

    def handle_export_audio(self, chapter_name):
        """Export audio segment to CRX folder"""
        from urllib.parse import unquote

        chapter_name = unquote(chapter_name)
        print(f"Export audio request for chapter: {chapter_name}")

        # Read POST data
        content_length = int(self.headers['Content-Length'])
        post_data = self.rfile.read(content_length)
        params = json.loads(post_data.decode('utf-8'))

        start_time = float(params['start'])
        end_time = float(params['end'])
        sentence_id = params.get('sentenceId', 'unknown')

        # Find the audio file - try chapter folder first, then book root
        audio_path = BASE_DIR / chapter_name / f"{chapter_name}.treated.wav"
        if not audio_path.exists():
            audio_path = BASE_DIR / chapter_name / f"{chapter_name}.wav"
        if not audio_path.exists():
            audio_path = BASE_DIR / f"{chapter_name}.wav"

        print(f"Looking for audio at: {audio_path}")
        print(f"Audio exists: {audio_path.exists()}")

        if not audio_path.exists():
            self.send_json_response({'error': 'Audio file not found'}, 404)
            return

        try:
            # Create CRX directory if it doesn't exist
            crx_dir = BASE_DIR / CRX_DIR_NAME
            crx_dir.mkdir(exist_ok=True)

            # Determine the next error number by checking existing audio files
            # Find the highest numbered .wav file in the CRX directory
            existing_files = list(crx_dir.glob('*.wav'))
            if existing_files:
                # Extract numbers from filenames like "001.wav", "002.wav"
                error_numbers = []
                for f in existing_files:
                    match = re.match(r'(\d+)\.wav', f.name)
                    if match:
                        error_numbers.append(int(match.group(1)))

                if error_numbers:
                    error_num = max(error_numbers) + 1
                else:
                    error_num = 1
            else:
                error_num = 1

            # Generate filename: ErrorNum.wav (e.g., 001.wav)
            export_filename = f"{error_num:03d}.wav"
            export_path = crx_dir / export_filename

            # Extract segment using ffmpeg
            duration = end_time - start_time
            cmd = [
                'ffmpeg',
                '-i', str(audio_path),
                '-ss', str(start_time),
                '-t', str(duration),
                '-c', 'copy',
                '-y',
                str(export_path)
            ]

            result = subprocess.run(cmd, capture_output=True, text=True)

            if result.returncode != 0:
                raise Exception(f"ffmpeg failed: {result.stderr}")

            self.send_json_response({
                'success': True,
                'filename': export_filename,
                'errorNumber': error_num,
                'path': str(export_path.relative_to(BASE_DIR))
            })

        except Exception as e:
            print(f"Error exporting audio: {e}")
            print(traceback.format_exc())
            self.send_json_response({'error': str(e)}, 500)

    def handle_add_to_crx(self, chapter_name):
        """Add entry to CRX Excel file"""
        from urllib.parse import unquote

        chapter_name = unquote(chapter_name)

        # Read POST data
        content_length = int(self.headers['Content-Length'])
        post_data = self.rfile.read(content_length)
        params = json.loads(post_data.decode('utf-8'))

        start_time = float(params['start'])
        end_time = float(params['end'])
        sentence_id = params.get('sentenceId', 'unknown')
        error_type = params.get('errorType', DEFAULT_ERROR_TYPE)
        comments = params.get('comments', '')
        padding_ms = params.get('paddingMs', 50)  # Default 50ms if not provided

        try:
            # Create CRX directory if it doesn't exist
            crx_dir = BASE_DIR / CRX_DIR_NAME
            crx_dir.mkdir(exist_ok=True)

            # CRX file path
            crx_filename = f"{BASE_DIR.name}_CRX.xlsx"
            crx_path = crx_dir / crx_filename

            # Determine next error number by checking existing audio files
            existing_files = list(crx_dir.glob('*.wav'))
            if existing_files:
                error_numbers = []
                for f in existing_files:
                    match = re.match(r'(\d+)\.wav', f.name)
                    if match:
                        error_numbers.append(int(match.group(1)))
                error_num = max(error_numbers) + 1 if error_numbers else 1
            else:
                error_num = 1

            # If CRX doesn't exist, copy BASE_CRX template
            if not crx_path.exists():
                if not CRX_TEMPLATE_PATH.exists():
                    raise Exception(f"CRX template not found: {CRX_TEMPLATE_PATH}")
                # Use shutil.copy2 to preserve metadata and formatting
                shutil.copy2(CRX_TEMPLATE_PATH, crx_path)

            # Use openpyxl to modify cells
            wb = openpyxl.load_workbook(crx_path)
            ws = wb.active

            # Find the row corresponding to this error number
            # Row number = CRX_DATA_ROW_START + (error_num - 1)
            target_row = CRX_DATA_ROW_START + (error_num - 1)

            # Format timecode as hh:mm:ss
            hours = int(start_time // 3600)
            minutes = int((start_time % 3600) // 60)
            seconds = int(start_time % 60)
            timecode = f"{hours:02d}:{minutes:02d}:{seconds:02d}"

            # Ensure Error # is populated in this row (Column B = 2)
            current_error = ws.cell(row=target_row, column=2).value
            if not current_error or not re.search(r'(\d+)', str(current_error)):
                ws.cell(row=target_row, column=2).value = f"{error_num:03d}"

            # Write data to cells
            # Column C: Recording Day (leave empty)
            # Column D: Chapter/Section
            ws.cell(row=target_row, column=4).value = chapter_name
            # Column E: PDF/Word Page # (leave empty)
            # Column F: File Timecode
            ws.cell(row=target_row, column=6).value = timecode
            # Column G: Error Type
            ws.cell(row=target_row, column=7).value = error_type
            # Column H: Comments
            ws.cell(row=target_row, column=8).value = comments

            # Save workbook
            wb.save(crx_path)

            # Export the audio segment as {error_num:03d}.wav
            audio_path = BASE_DIR / chapter_name / f"{chapter_name}.treated.wav"
            if not audio_path.exists():
                audio_path = BASE_DIR / chapter_name / f"{chapter_name}.wav"
            if not audio_path.exists():
                audio_path = BASE_DIR / f"{chapter_name}.wav"

            if audio_path.exists():
                audio_filename = f"{error_num:03d}.wav"
                audio_output_path = crx_dir / audio_filename

                # Use ffmpeg to extract the segment with dynamic tail padding
                padding_sec = padding_ms / 1000.0
                duration = (end_time - start_time) + padding_sec
                cmd = [
                    'ffmpeg',
                    '-i', str(audio_path),
                    '-ss', str(start_time),
                    '-t', str(duration),
                    '-c', 'copy',
                    '-y',
                    str(audio_output_path)
                ]

                result = subprocess.run(cmd, capture_output=True, text=True)
                if result.returncode != 0:
                    print(f"Warning: ffmpeg failed to export audio: {result.stderr}")
            else:
                print(f"Warning: Audio file not found for {chapter_name}")

            self.send_json_response({
                'success': True,
                'errorNumber': error_num,
                'crxFile': crx_filename,
                'timecode': timecode,
                'audioFile': f"{error_num:03d}.wav"
            })

        except Exception as e:
            print(f"Error adding to CRX: {e}")
            print(traceback.format_exc())
            self.send_json_response({'error': str(e)}, 500)

    def send_json_response(self, data, status=200):
        """Helper to send JSON response"""
        self.send_response(status)
        self.send_header('Content-type', 'application/json')
        self.end_headers()
        self.wfile.write(json.dumps(data).encode())

    def log_message(self, format, *args):
        """Custom log format"""
        print(f"[{self.log_date_time_string()}] {format % args}")


def main():
    # Parse command-line arguments (override globals)
    global BASE_DIR, PORT
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

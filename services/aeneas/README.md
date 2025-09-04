# Aeneas Alignment Service

HTTP microservice that wraps `aeneas.tools.execute_task` for forced alignment of audio chunks with text lines.

## Prerequisites

- Python 3.9 (recommended: `C:\aeneas-install\python39\python.exe`)
- Aeneas library installed in the Python environment
- FFmpeg available in PATH or via `FFMPEG_EXE` environment variable

## Environment Variables

- `AENEAS_HOME`: Path to Aeneas installation (e.g., `C:\aeneas-install`)
- `AENEAS_PYTHON`: Python executable with Aeneas (e.g., `C:\aeneas-install\python39\python.exe`)
- `FFMPEG_EXE`: Path to FFmpeg executable (optional, defaults to `ffmpeg` in PATH)

### Windows PowerShell Setup
```powershell
$env:AENEAS_HOME = "C:\aeneas-install"
$env:AENEAS_PYTHON = "C:\aeneas-install\python39\python.exe"
$env:FFMPEG_EXE = "C:\Program Files\ffmpeg\bin\ffmpeg.exe"
```

### WSL Bash Setup
```bash
export AENEAS_HOME=/mnt/c/aeneas-install
export AENEAS_PYTHON=/mnt/c/aeneas-install/python39/python.exe
export FFMPEG_EXE=ffmpeg
```

## Installation

1. Create a Python virtual environment:
```bash
python -m venv venv39
source venv39/bin/activate  # Linux/WSL
# or
venv39\Scripts\activate     # Windows
```

2. Install dependencies:
```bash
pip install -r requirements.txt
```

## Running the Service

### Development Server (uvicorn)
```bash
uvicorn app.main:app --host 0.0.0.0 --port 8082 --reload
```

### Production
```bash
uvicorn app.main:app --host 0.0.0.0 --port 8082 --workers 1
```

## API Endpoints

### Health Check
```http
POST /v1/health
```

Returns:
```json
{
  "ok": true,
  "python_version": "3.9.18",
  "aeneas_version": "1.7.3",
  "service": "aeneas-alignment",
  "timestamp": "2024-09-04T10:30:00.000Z"
}
```

### Align Chunk
```http
POST /v1/align-chunk
Content-Type: application/json

{
  "chunk_id": "chunk_001",
  "audio_path": "C:\\audio\\chunk_001.wav",
  "lines": ["Hello world.", "This is a test."],
  "language": "eng",
  "timeout_sec": 600
}
```

Returns:
```json
{
  "chunk_id": "chunk_001",
  "fragments": [
    {"begin": 0.0, "end": 1.5},
    {"begin": 1.5, "end": 3.0}
  ],
  "counts": {
    "lines": 2,
    "fragments": 2
  },
  "tool": {
    "python": "3.9.18",
    "aeneas": "1.7.3"
  },
  "generated_at": "2024-09-04T10:30:15.123Z"
}
```

## Path Handling

The service automatically normalizes Windows and WSL paths:
- Windows: `C:\audio\file.wav`
- WSL: `/mnt/c/audio/file.wav`

Both formats are accepted and converted internally as needed.

## Testing

Run a quick smoke test:
```bash
curl -X POST "http://localhost:8082/v1/health"
```

## Troubleshooting

1. **Aeneas not found**: Ensure `AENEAS_PYTHON` points to the correct Python executable with Aeneas installed.

2. **Permission denied**: On WSL, make sure the Windows paths are accessible and the Python executable has proper permissions.

3. **Timeout errors**: Increase `timeout_sec` for longer audio files or complex alignments.

4. **Audio format issues**: Ensure audio files are in WAV format. Other formats may not be supported by Aeneas.

## Integration

This service is designed to be called from the AMS pipeline `AlignChunksStage`:
- Receives chunk audio files and text lines
- Returns time-aligned fragments for each text line  
- Supports chunk-relative timing (converted to chapter time in the RefineStage)
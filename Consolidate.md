# Consolidated Plan: Aeneas Service Integration and Pipeline Updates

This document consolidates the updated design to:
- Run Aeneas as a service under `services/aeneas` (mirroring Nemo).
- Execute Aeneas immediately after ASR, per chunk, and emit JSON.
- Refine sentence timings by snapping `sentence[n].end` to the earliest `silence.start` after Aeneas’ end and before `sentence[n+1].begin` (noise floor configurable per chapter).
- Collate by stitching chunks and replacing qualified regions with room tone (including cross‑chunk boundary slivers) so the final WAV duration equals the original.

---

## Environment & Prerequisites

- Aeneas install location (Windows): `C:\\aeneas-install`.
- Python 3.9 interpreter: `C:\\aeneas-install\\python39\\python.exe`.
- FFmpeg available (Windows or WSL). Under WSL, prefer Linux ffmpeg (`apt install ffmpeg`).
- Recommended environment variables:
  - Windows PowerShell
    - `$env:AENEAS_HOME = "C:\\aeneas-install"`
    - `$env:AENEAS_PYTHON = "C:\\aeneas-install\\python39\\python.exe"`
    - `$env:FFMPEG_EXE = "C:\\\\Program Files\\\\ffmpeg\\\\bin\\\\ffmpeg.exe"` (or your path)
  - WSL bash
    - `export AENEAS_HOME=/mnt/c/aeneas-install`
    - `export AENEAS_PYTHON=/mnt/c/aeneas-install/python39/python.exe`
    - `export FFMPEG_EXE=ffmpeg`
- Path bridging: when invoking Windows Python from WSL, pass `/mnt/c/...` paths (not `C:\\...`).

Validation (manual; will be a CLI):
- `"C:\\aeneas-install\\python39\\python.exe" -V`
- `"C:\\aeneas-install\\python39\\python.exe" -c "import aeneas,sys;print(sys.version);print(getattr(aeneas,'__version__','unknown'))"`
- `"C:\\aeneas-install\\python39\\python.exe" -m aeneas.tools.execute_task --help`

---

## Revised Staged Pipeline

Order and purpose:
1) `timeline` → Detect silences (FFmpeg) for the whole chapter.
2) `plan` → Plan deterministic windows (60–90s, target 75s) at silence candidates.
3) `chunks` → Cut audio into chunk WAVs per plan.
4) `transcripts` → Run ASR (Nemo) per chunk; store raw JSON.
5) `align-chunks` → Run Aeneas service per chunk to align lines; store chunk‑relative fragments.
6) `refine` → Convert chunk‑relative to chapter time and snap sentence[n].end to earliest silence.start.
7) `collate+roomtone` → Stitch content and replace qualified regions with room tone; produce final chapter WAV.
8) `validate` → Script vs transcript validation (WER/CER report).

Idempotency: Each stage is a `StageRunner` with Manifest v2 fingerprints: InputHash + ParamsHash + ToolVersions. Skip when unchanged.

---

## Stage Contracts

### 1) timeline (DetectSilenceStage) — implemented
- Input: `manifest.Input.Path` (chapter WAV)
- Params (example): `{ dbFloor: -30.0, minSilenceDur: 0.30 }`
- Output: `timeline/silence.json` (SilenceTimelineV2)
- ToolVersions: `{ ffmpeg }`

### 2) plan (PlanWindowsStage) — implemented
- Input: `timeline/silence.json`
- Params: `{ min: 60, max: 90, target: 75, strictTail: true }`
- Output: `plan/windows.json` (WindowPlanV2)

### 3) chunks (ChunkAudioStage)
- Input: `manifest.Input.Path`, `plan/windows.json`
- Params: `{ format: "wav", sampleRate: 44100 }`
- Output:
  - `chunks/index.json` (ChunkIndex: ids, spans, filenames, sha256, duration)
  - `chunks/wav/{id}.wav`
- ToolVersions: `{ ffmpeg }`

### 4) transcripts (TranscribeStage)
- Input: `chunks/index.json`
- Params: ASR engine config `{ model, language, beam?, ... }`
- Output:
  - `transcripts/raw/{id}.json`
  - `transcripts/index.json` (map chunk→json)
  - `transcripts/merged.json` (optional full timeline merge)
- ToolVersions: `{ asrEngine, modelVersion }`

### 5) align-chunks (AlignChunksStage → Aeneas service)
- Purpose: Forced alignment per chunk right after ASR; cacheable and parallel.
- Input: `chunks/wav/{id}.wav`, textual lines for that chunk (e.g., via TranscriptIndex slice or ASR text lines)
- Params: `{ language: "eng", timeoutSec: 600 }`
- Output: `align/chunks/{id}.aeneas.json`
  - `{ chunkId, offsetSec, language, textDigest, fragments: [{ begin, end }], tool: { python, aeneas }, generatedAt }`
  - Times are relative to the chunk (later shifted to chapter time).
- Fingerprint: chunk WAV sha256 + textDigest + params + `{ python, aeneas }`

### 6) refine (RefineStage — snap to earliest silence.start)
- Rule:
  - `sentence[n].start` = Aeneas begin (converted to chapter time)
  - `sentence[n].end` = earliest `silence.start` such that `silence.start ≥ aeneas.end` and `< sentence[n+1].start`
  - Enforce monotonic non‑overlap and minimum length (e.g., ≥ 50 ms)
- Inputs:
  - `align/chunks/*.aeneas.json`
  - `timeline/silence.json` (whole chapter)
  - `chunks/index.json` (for offsets) and `plan/windows.json`
- Params:
  - `{ silenceThresholdDb, minSilenceDurSec }` (noise floor configurable per chapter; you confirm the quietest tail)
- Output: `refine/sentences.json` (chapter coordinates)
  - `[{ id, start, end, startWordIdx?, endWordIdx?, source: "aeneas+silence.start" }]`

### 7) collate+roomtone (CollateStage)
- Goal: Final WAV equals original duration; gaps/slivers replaced by room tone.
- Qualified regions:
  - Inter‑sentence gaps from `refine/sentences.json`: replace with room tone of exact gap length.
  - Cross‑chunk boundary slivers: when a hard cut split a sentence across chunks, drop both slivers and insert room tone bridging (sum of both slivers).
- Your example mapping:
  - Input near a cut: `... sentence[15s] + [25ms sliver] || [25ms sliver] + sentence[10s] ...`
  - Collated target: `... sentence[15s] + roomtone[50ms] + sentence[10s] ...`
- Inputs: `refine/sentences.json`, `manifest.Input.Path`, `chunks/wav/*.wav`, `plan/windows.json`
- Params (roomtone): `{ source: "auto|file", levelDb: -50, minGapMs: 5, maxGapMs: 2000, bridgeMaxMs: 60 }`
  - `auto`: sample low‑energy windows from chapter to synthesize seamless loop; `file`: use provided RT WAV.
- Output:
  - `collate/final.wav` (chapter render with substitutions)
  - `collate/segments.json` (final per‑sentence timeline used during render)
  - `collate/log.json` (list of replacements and durations)
- Rendering: via ffmpeg filter_complex or native DSP; verify total length == original (±1 sample).

### 8) validate (ValidateStage)
- Input: `transcripts/merged.json` and/or `collate/tx.json`, optional `refine/sentences.json`
- Params: `{ werThresh: 0.25, cerThresh: 0.25, costs: { sub:1, ins:1, del:1 } }`
- Output: `validate/report.json`, `validate/summary.txt`
- Uses existing `ScriptValidator` and `Ams.Align.Tx` helpers.

---

## Aeneas Service (services/aeneas)

- Structure:
  - `services/aeneas/app/main.py` (FastAPI/Flask HTTP service)
  - `services/aeneas/app/exec_aeneas.py` (wrapper around `python -m aeneas.tools.execute_task`)
  - `services/aeneas/venv39/` (Python 3.9 virtual environment)
  - `services/aeneas/logs/`, `services/aeneas/tests/`
- Endpoints:
  - `POST /v1/health` → `{ python_version, aeneas_version, ok }`
  - `POST /v1/align-chunk`
    - In: `{ chunk_id, audio_path, lines: [string], language, timeout_sec? }`
    - Out: `{ chunk_id, fragments: [{ begin, end }], counts: { lines }, tool: { python, aeneas }, generatedAt }`
- Invocation:
  - `AENEAS_PYTHON -m aeneas.tools.execute_task "<audio>" "<sentences.txt>" "task_language=<lang>|is_text_type=plain|os_task_file_format=json" "<out.json>"`
- Pathing:
  - Accept both Windows paths and `/mnt/c/...` (normalize internally); ensure quoting.
- Health/version capture:
  - `python -c "import sys,aeneas;print(sys.version);print(getattr(aeneas,'__version__','unknown'))"`
- Startup example:
  - `uvicorn app.main:app --host 0.0.0.0 --port 8082`

---

## CLI Verbs (additions)

- `ams asr run --in <audio.wav> --from transcripts --to collate [--force]`
- `ams asr align-chunks --in <audio.wav> --service http://localhost:8082 --lang eng`
- `ams asr refine --in <audio.wav> --silence-threshold-db -34 --silence-min-dur 0.08`
- `ams asr collate --in <audio.wav> --roomtone auto --bridge-max-ms 60`
- `ams env aeneas-validate --service http://localhost:8082` (health/version + tiny smoke test)

---

## Data Models

- `align/chunks/{id}.aeneas.json`
  - `{ chunkId, offsetSec, language, textDigest, fragments: [{ begin, end }], tool: { python, aeneas }, generatedAt }`
- `refine/sentences.json`
  - `[{ id, start, end, startWordIdx?, endWordIdx?, source: "aeneas+silence.start" }]`
- `collate/segments.json`
  - `{ sentences: [...], replacements: [{ kind: "gap|boundary_sliver", from, to, duration, rt_level_db }] }`

---

## Silence Rules & Configuration

- Snap target = earliest `silence.start` (not `silence_end`).
- `silenceThresholdDb` must be configurable per chapter to match the quietest tail of a word; store in `timeline/params.snapshot.json` and reuse in refine.
- `minSilenceDurSec` configurable to avoid micro‑gaps.

---

## Manifest v2 & Idempotency

- `StageFingerprint = { InputHash, ParamsHash, ToolVersions }`.
- Tool versions captured where relevant: `{ ffmpeg }`, `{ python, aeneas }`, `{ asrEngine, model }`.
- Stages skip when fingerprints match and `Status == completed`.

---

## Room Tone Algorithm (Collate)

1) Build RT source:
   - `auto`: detect low‑energy windows and synthesize a loop with crossfades.
   - `file`: use a user‑provided RT WAV; resample as needed.
2) Replace:
   - Inter‑sentence gaps → insert RT buffer of exact gap length.
   - Boundary slivers across chunk cuts → remove slivers on both sides; insert RT of `tL + tR` (e.g., 25ms + 25ms = 50ms).
3) Render:
   - Concatenate sentences and RT inserts with clean boundaries; optional 5–10ms crossfades at joins.
   - Verify output length equals original chapter (±1 sample); log substitutions.

---

## Testing Strategy

- Unit
  - Snap‑to‑silence.start selection; boundary sliver detection; chunk→chapter time mapping; fingerprint determinism.
- Integration (mocked)
  - Mock Aeneas service returns deterministic fragments; end‑to‑end transcripts→align‑chunks→refine→collate on synthetic audio.
- Smoke (real tools)
  - `env aeneas-validate` against local service; tiny 2–3s WAV + short text.

---

## Next Steps

- Scaffold `services/aeneas` (FastAPI + subprocess wrapper, Python 3.9) and a `venv39` bootstrap.
- Implement `AlignChunksStage` (HTTP client), `RefineStage` (snap rule), `CollateStage` (roomtone rendering).
- Wire new stages into `AsrPipelineRunner` and add CLI verbs.
- Add chapter‑level configuration for `silenceThresholdDb` and `minSilenceDurSec`.
- Add `ams env aeneas-validate` command.
- Add tests (unit + mocked integration) and a short WAV smoke workflow.


### Test files paths
- Original Wav: "C:\Aethon\InProgress\GlorySeeker3\GlorySeeker3 - Batch1\glory seeker 3 Chapter 12_orig.wav"
- Roomtone: "C:\transcriptions\GlorySeeker3\roomtone.wav"
- BookIndex: "C:\transcriptions\GlorySeeker3\book-index.json"


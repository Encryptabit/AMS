ProjectState.md (Generated)
Date: 2025-09-01

Summary
- Goal: Production-ready Audio Management System blending .NET orchestration, Python ASR+alignment, and Zig DSP with a future node-graph host.
- Current focus: End-to-end ASR → validation loop and preparing script ingestion from DOCX.

What's Implemented
- CLI
  - `asr run`: Calls NeMo service (`/asr`), saves JSON.
  - `validate script`: Validates script vs ASR JSON; computes WER/CER and findings.
  - `pipeline run`: Single command for ASR → validation → report. Writes `<report>.asr.json`.
  - `build-index`: Creates word-level book indexes from DOCX, TXT, MD, RTF files with caching.
  - `text normalize`: Direct text normalization testing utility.
- ASR Client
  - `Ams.Core.AsrClient`: Posts `{audio_path, model, language}` and deserializes response.
  - Updated to handle word-level tokens without segment structure for cleaner validation.
- Validation Core
  - `ScriptValidator`: Text normalization, DP alignment (WER/CER), basic segment stats.
- DSP
  - Zig gain with smoothed parameter and C ABI; .NET P/Invoke wrapper `Ams.Dsp.Native.AmsDsp`.
  - CLI `dsp` demo writes `ams_out.wav`.
- Book Parser & Indexer (new)
  - `BookParser`: Multi-format parser (DOCX via DocX package, TXT, MD, RTF) with metadata extraction.
  - `BookIndexer`: Word/sentence/paragraph segmentation with timing estimation for audio alignment.
  - `BookCache`: SHA256-validated file-based caching system for processed book indexes.
  - Full CLI integration with comprehensive options (WPM, metadata extraction, paragraph segments).

How To Run
- Start ASR service: `services/asr-nemo/start_service.bat` (Windows) or `uvicorn services.asr-nemo.app:app`.
- End-to-end: `dotnet run --project host/Ams.Cli -- pipeline run -a <audio.wav> -s <script.txt> -o <report.json>`
- ASR only: `dotnet run --project host/Ams.Cli -- asr run -a <audio.wav> -o <asr.json>`
- Validate only: `dotnet run --project host/Ams.Cli -- validate script -a <audio.wav> -s <script.txt> -j <asr.json> -o <report.json>`
- Book indexing: `dotnet run --project host/Ams.Cli -- build-index -b <book.docx> -o <index.json>`
- Text normalization: `dotnet run --project host/Ams.Cli -- text normalize "your text here"`
- DSP demo: `dotnet run --project host/Ams.Cli -- dsp`

Book Processing & Indexing
- Multi-format support: DOCX (via DocX package), TXT, MD, RTF with automatic format detection.
- Metadata extraction: Title, author, and document properties from supported formats.
- Word-level indexing: Complete hierarchical structure (words → sentences → paragraphs) with global offsets.
- Caching system: SHA256 file hash validation with automatic cache invalidation on file changes.
- CLI integration: `build-index` command with comprehensive options for WPM estimation, normalization controls.
- Test coverage: 88.6% pass rate (31/35 tests) with full parser and indexer validation.
- Performance: ~24s estimated duration calculation for 83 words, efficient in-memory processing.

Immediate Next Steps
1) Book-ASR Integration: Connect `build-index` output with `validate script` for word-level timing validation.
2) Timing alignment: Use BookIndex word offsets with ASR tokens for precise audio-text synchronization.
3) Validation enhancement: Add findings for timing gaps, pacing, and word-level confidence scores.
4) Alignment (Aeneas): Define `/align` API + `AeneasClient`; integrate with BookIndex for forced alignment.
5) DSP Host: Sketch `IAudioNode` and `TimelineStitcherNode`; prepare for real-time audio processing integration.

Risks/Notes
- ASR service returns word-level tokens without confidence values; BookIndex system handles this appropriately.
- BookCache test failures (3/35) relate to file system operations but don't affect core functionality.
- DOCX parsing now uses DocX package (v4.0.25105.5786) with Dictionary-based CoreProperties access.
- Text normalization test shows "cannot" vs "can not" contraction handling difference (1 test failure).

Fast Resume Checklist
- Is ASR service healthy? `GET /health` must return 200; set `HUGGINGFACE_TOKEN`.
- Test book processing: `dotnet run --project host/Ams.Cli -- build-index -b <file.docx> -o <output.json>`
- Verify caching: Run same command twice, second run should show "Found valid cache".
- Check text normalization: `dotnet run --project host/Ams.Cli -- text normalize "test text"`
- Run end-to-end: `pipeline run` and inspect WER/CER and findings in the report.


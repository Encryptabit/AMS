ProjectState.md (Generated)
Date: 2025-09-01

Summary
- Goal: Production-ready Audio Management System blending .NET orchestration, Python ASR+alignment, and Zig DSP with a future node-graph host.
- Current focus: End-to-end ASR → validation loop and preparing script ingestion from DOCX.

What’s Implemented
- CLI
  - `asr run`: Calls NeMo service (`/asr`), saves JSON.
  - `validate script`: Validates script vs ASR JSON; computes WER/CER and findings.
  - `pipeline run`: Single command for ASR → validation → report. Writes `<report>.asr.json`.
- ASR Client
  - `Ams.Core.AsrClient`: Posts `{audio_path, model, language}` and deserializes response.
  - Confidence handling is optional (tokens/segments use nullable confidence).
- Validation Core
  - `ScriptValidator`: Text normalization, DP alignment (WER/CER), basic segment stats.
- DSP
  - Zig gain with smoothed parameter and C ABI; .NET P/Invoke wrapper `Ams.Dsp.Native.AmsDsp`.
  - CLI `dsp` demo writes `ams_out.wav`.
- DOCX Extraction (new)
  - `Ams.Core.DocxScriptExtractor`: Extracts relevant text from DOCX with style/pattern filters.

How To Run
- Start ASR service: `services/asr-nemo/start_service.bat` (Windows) or `uvicorn services.asr-nemo.app:app`.
- End-to-end: `dotnet run --project host/Ams.Cli -- pipeline run -a <audio.wav> -s <script.txt> -o <report.json>`
- ASR only: `dotnet run --project host/Ams.Cli -- asr run -a <audio.wav> -o <asr.json>`
- Validate only: `dotnet run --project host/Ams.Cli -- validate script -a <audio.wav> -s <script.txt> -j <asr.json> -o <report.json>`
- DSP demo: `dotnet run --project host/Ams.Cli -- dsp`

Script Ingestion From DOCX
- Use `DocxScriptExtractor.ExtractRelevantText(path, options)` to produce normalized text.
- Defaults: includes chapter headings (matching “Chapter <n>”, Prologue/Epilogue), excludes headers/footers/TOC/footnotes, collapses whitespace.
- Next: Add CLI flag `--script-docx <file.docx>` to convert on the fly (pending).

Immediate Next Steps
1) CLI: Add `--script-docx` to `validate script` and `pipeline run` to auto-extract text via `DocxScriptExtractor`.
2) Alignment (Aeneas): Define `/align` API + `AeneasClient`; optionally enable via `--align-service`.
3) Timing checks: Add findings for long/short gaps and pacing based on aligned word timings.
4) Tests: Add golden fixtures for text, ASR JSON, and timing-gap cases; gate ASR/Aeneas integration tests via env flag.
5) DSP Host: Sketch `IAudioNode` and `TimelineStitcherNode`; wire `AmsDsp` gain as first node.

Risks/Notes
- NeMo confidences often unavailable; treat confidence as optional and rely on WER/CER/alignment timing.
- DOCX styles vary by publisher; adjust `ExcludeStyleIds` and heading patterns per title.

Fast Resume Checklist
- Is ASR service healthy? `GET /health` must return 200; set `HUGGINGFACE_TOKEN`.
- Convert script from DOCX using `DocxScriptExtractor` in a short snippet or new CLI flag.
- Run `pipeline run` and inspect WER/CER and findings in the report.


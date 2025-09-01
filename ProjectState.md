ProjectState.md (Generated)
Date: 2025-09-01

Summary
- Goal: Production-ready Audio Management System blending .NET orchestration, Python ASR+alignment, and Zig DSP with a future node-graph host.
- Current focus: Canonical book decoding and a slim, deterministic BookIndex with exact source fidelity (no normalization at rest).

What's Implemented
- CLI
  - `asr run`: Calls NeMo service (`/asr`), saves JSON.
  - `validate script`: Validates script vs ASR JSON; computes WER/CER and findings.
  - `build-index`: Creates canonical book indexes from DOCX, TXT, MD, RTF files with caching.
  - `book verify`: Read-only doctor for `BookIndex` JSON. Checks counts parity, ordering/coverage of ranges, flags apostrophe-split and TOC-burst warnings, and prints a deterministic hash of the canonical JSON. Exits non-zero on failure (CI-friendly). 
  - `text normalize`: Direct text normalization testing utility.
- ASR Client
  - `Ams.Core.AsrClient`: Posts `{audio_path, model, language}` and deserializes response.
  - Updated to handle word-level tokens without segment structure for cleaner validation.
- Validation Core
  - `ScriptValidator`: Text normalization, DP alignment (WER/CER), basic segment stats.
- DSP
  - Zig gain with smoothed parameter and C ABI; .NET P/Invoke wrapper `Ams.Dsp.Native.AmsDsp`.
  - CLI `dsp` demo writes `ams_out.wav`.
- Book Parser & Indexer (canonical)
  - `BookParser`: Multi-format reader (DOCX/TXT/MD/RTF) that extracts paragraphs without normalization; derives paragraph `style` (DocX `StyleId`) and best-effort `kind` (Heading/Body). Title/author read when available.
  - `BookIndexer`: Canonical tokenizer (whitespace split; punctuation preserved), builds `words[]` and structure ranges: `sentences[]`, `paragraphs[]`. No timing/confidence in BookIndex.
  - `BookCache`: SHA256-validating cache keyed by source file; reuse only when hash matches. JSON serialization deterministic.
  - CLI `build-index`: Slim options; prints canonical totals; no ASR alignment in this command.

How To Run
- Start ASR service: `services/asr-nemo/start_service.bat` (Windows) or `uvicorn services.asr-nemo.app:app`.
- ASR only: `dotnet run --project host/Ams.Cli -- asr run -a <audio.wav> -o <asr.json>`
- Validate only: `dotnet run --project host/Ams.Cli -- validate script -a <audio.wav> -s <script.txt> -j <asr.json> -o <report.json>`
- Book indexing: `dotnet run --project host/Ams.Cli -- build-index -b <book.docx> -o <index.json>`
- Book doctor (read-only): `dotnet run --project host/Ams.Cli -- book verify --index <index.json>`
- Text normalization: `dotnet run --project host/Ams.Cli -- text normalize "your text here"`
- DSP demo: `dotnet run --project host/Ams.Cli -- dsp`

Book Processing & Indexing
- Canonical fidelity: No normalization at rest. Tokens preserve exact casing, punctuation, apostrophes, hyphens, and Unicode.
- Structure: `sentences[]` and `paragraphs[]` are inclusive word index ranges that cover all words contiguously without overlap.
- Slim schema: `words[]` has only `{text, wordIndex, sentenceIndex, paragraphIndex, [charStart?], [charEnd?]}`; no timing/confidence.
- Totals: `totals = { words, sentences, paragraphs, estimatedDurationSec }` computed without mutating tokens.
- Caching: SHA256 of raw file bytes; reuse only on hash match (and future parser version key if added). Deterministic JSON bytes.
- Doctor: `book verify` never mutates the index; if checks fail, keep the BookIndex canonical—adjust decoder tokenization (sentence/paragraph walking, apostrophes) and paragraph style classification (DocX) instead.
- Tests: All tests passing (29/29), including canonical round‑trip and slimness checks.

Immediate Next Steps
1) Parser version in cache key: add version suffix in cache filenames for strict reuse across parser changes.
2) Optional char ranges: compute `charStart/charEnd` cheaply from source spans (omit when not available).
3) Sections/front matter: optional `sections[]` with cautious heuristics; populate `buildWarnings[]` for low-confidence detections.
4) ASR alignment remains separate: timing/confidence stay out of BookIndex; continue via `ScriptValidator` and future alignment utilities.
5) DSP Host: Sketch `IAudioNode` and `TimelineStitcherNode`; prepare for real-time audio processing integration.
 6) Doctor output for CI: add `--json` to emit a machine-readable verification report; consider warning categories (apostrophes, hyphenation, page-number runs) with counts.

Risks/Notes
- DOCX style: using `Paragraph.StyleId` to avoid API obsolescence; style/kind are best‑effort.
- Tokenization: whitespace-only preserves punctuation with tokens (e.g., `test.”`), which is intentional for canonical fidelity.
- Determinism: cache JSON settings are stable; ensure environment does not inject non-deterministic timestamps beyond `indexedAt`.
 - Verification scope: `book verify` depends on the canonical schema (`Ams.Core.BookIndex`). If schema evolves, update the doctor to maintain stable hashing and checks.

Fast Resume Checklist
- Test canonical index: `dotnet run --project host/Ams.Cli -- build-index -b <file.docx> -o <index.json>`; rerun to verify "Found valid cache" and identical JSON bytes.
- Spot-check canonical fidelity: inspect first ~200 tokens for exact text (quotes/dashes/apostrophes preserved).
- Validate script workflow: `validate script` remains unchanged and separate from BookIndex.
- Verify index health: `dotnet run --project host/Ams.Cli -- book verify --index <index.json>`; expect OK + deterministic hash. In CI, non-zero exit signals issues to investigate upstream (tokenization/styles), not to auto-fix.

ProjectState.md (Generated)
Date: 2025-09-03

Summary
- Goal: Production-ready Audio Management System blending .NET orchestration, Python ASR+alignment (services), and Zig/C# DSP with a future node-graph host.
- Current focus: Canonical book decoding, section-aware alignment (n-gram anchors), sentence-only refinement (Aeneas starts + FFmpeg ends), and clean roomtone rendering between sentences.

What's Implemented
- CLI
  - `asr run`: Calls NeMo service (`/asr`), saves JSON.
  - `validate script`: Validates script vs ASR JSON; computes WER/CER and findings.
  - `build-index`: Creates canonical book indexes from DOCX, TXT, MD, RTF files with caching.
  - `book verify`: Read-only doctor for `BookIndex` JSON. Checks counts parity, ordering/coverage of ranges, flags apostrophe-split and TOC-burst warnings, and prints a deterministic hash of the canonical JSON. Exits non-zero on failure (CI-friendly). 
  - `text normalize`: Direct text normalization testing utility.
  - `align anchors`: Computes n-gram anchors between a BookIndex and ASR JSON; optional section detection, tunable policy knobs, and optional windows output.
  - `align tx`: Emits `TranscriptIndex (*.tx.json)` mapping sentences/paragraphs to ASR token ranges (script ranges), optionally scoped to a detected section.
  - `refine-sentences`: Sentence-only refinement. Starts from Aeneas (per-sentence) and ends from FFmpeg `silencedetect` (noise floor and min duration are parameters). Outputs `[ { start, end, startWordIdx, endWordIdx } ]`.
  - Roomtone renderer (scripts): `scripts/RoomtoneCli` renders 44.1 kHz 16‑bit WAV with roomtone fills and 5 ms crossfades.
- ASR Client
  - `Ams.Core.AsrClient`: Posts `{audio_path, model, language}` and deserializes response.
  - Updated to handle word-level tokens without segment structure for cleaner validation.
- Validation Core
  - `ScriptValidator`: Text normalization, DP alignment (WER/CER), basic segment stats.
- Alignment & Anchors
  - `AnchorDiscovery`: N-gram based anchor selection with stopword filtering, sentence-boundary guarding, density control, and LIS-based monotonicity over ASR positions.
    - Unique n-grams first; if sparse, relax to two occurrences per side when ≥ `MinSeparation`, then fallback to smaller `n` (e.g., 3→2).
    - Content filter rejects anchors that start/end with stopwords and enforces minimum content tokens for trigrams+.
    - Windows builder returns half-open search windows between anchors with sentinels.
  - `SectionLocator`: Detects chapter/section by matching normalized ASR prefix to `BookIndex.sections[*].title` (e.g., “chapter fourteen”, “prologue”).
    - Provides `DetectSection` and `DetectSectionWindow` APIs.
  - Anchor selection overload that restricts to a book word-range `[start,end]` for section-scoped alignment.
  - Core components for reuse beyond CLI:
    - `AnchorTokenizer` (canonical normalization)
    - `AnchorPreprocessor` (book/ASR views, section window mapping)
    - `AnchorPipeline` (section detect → window map → anchors → windows)
- DSP
  - Zig gain with smoothed parameter and C ABI; .NET P/Invoke wrapper `Ams.Dsp.Native.AmsDsp`.
  - CLI `dsp` command orchestrates Plugalyzer chains (`dsp run`), manages plugin directories (`dsp set-dir`), caches parameter metadata (`dsp init`), lists cached plugins (`dsp list`), edits chain files (`dsp chain ...`), and controls session settings (`dsp output-mode`, `dsp overwrite`).
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
- DSP processing:
  - Configure plugin directories: `dotnet run --project host/Ams.Cli -- dsp set-dir add <path>`
  - Cache plugin metadata: `dotnet run --project host/Ams.Cli -- dsp init`
  - Inspect cached plugins: `dotnet run --project host/Ams.Cli -- dsp list`
  - Control defaults: `dotnet run --project host/Ams.Cli -- dsp output-mode source|post`, `dotnet run --project host/Ams.Cli -- dsp overwrite on|off`
  - Manage chain file: `dotnet run --project host/Ams.Cli -- dsp chain list [--chain dsp.chain.json]`, `... chain add --plugin <friendly-name>`
  - Run a chain: `dotnet run --project host/Ams.Cli -- dsp run -i <in.wav> [--chain dsp.chain.json | --plugin <friendly-name>]`
 - Anchors (n‑grams): `dotnet run --project host/Ams.Cli -- align anchors -i out\book.index.json -j out\asr.json [--detect-section true] [--ngram 3] [--emit-windows] [--out anchors.json]`

Sentence-Only Refinement & Roomtone
- Create TX per chapter:
  `dotnet run --project host/Ams.Cli -- align tx -i <book.index.json> -j <chapter.asr.json> -a <chapter.wav> -o <chapter.tx.json>`
- Refine sentence boundaries (PowerShell example):
  `dotnet .\host\Ams.Cli\bin\Debug\net9.0\Ams.Cli.dll refine-sentences -t <chapter.tx.json> -j <chapter.asr.json> -a <chapter.wav> -o <chapter.sentences.refined.json> --silence-threshold-db -38 --silence-min-dur 0.12`
- Render roomtone using refined sentences (PowerShell):
  `dotnet .\scripts\RoomtoneCli\bin\Release\net9.0\RoomtoneCli.dll "<chapter.tx.json>" "<chapter.roomtone.wav>" --sentences "<chapter.sentences.refined.json>" --sr 44100 --fade 5 --tone -60`

Book Processing & Indexing
- Canonical fidelity: No normalization at rest. Tokens preserve exact casing, punctuation, apostrophes, hyphens, and Unicode.
- Structure: `sentences[]` and `paragraphs[]` are inclusive word index ranges that cover all words contiguously without overlap.
- Slim schema: `words[]` has only `{text, wordIndex, sentenceIndex, paragraphIndex, [charStart?], [charEnd?]}`; no timing/confidence.
- Totals: `totals = { words, sentences, paragraphs, estimatedDurationSec }` computed without mutating tokens.
- Caching: SHA256 of raw file bytes; reuse only on hash match (and future parser version key if added). Deterministic JSON bytes.
- Doctor: `book verify` never mutates the index; if checks fail, keep the BookIndex canonical—adjust decoder tokenization (sentence/paragraph walking, apostrophes) and paragraph style classification (DocX) instead.
- Tests: All tests passing (35/35), including canonical round‑trip, slimness checks, anchor/section detection, and pipeline/preprocessor tests.

Immediate Next Steps
1) Parser version in cache key: add version suffix in cache filenames for strict reuse across parser changes.
2) Optional char ranges: compute `charStart/charEnd` cheaply from source spans (omit when not available).
3) Sections/front matter: optional `sections[]` with cautious heuristics; populate `buildWarnings[]` for low-confidence detections. (Implemented basic detection for alignment; keep improving title heuristics.)
4) Policy tuning and benchmarks:
   - Expose `NGram`, `TargetPerTokens`, `MinSeparation`, and `Stopwords` via config.
   - Add micro-bench and chapter-scale perf tests for indexing + LIS.
5) CLI output options:
   - Add `--emit-times` to include ASR token timing for each anchor.
   - Add `--emit-mapping` to include ASR filtered→original token index mapping.
6) Sentence refinement UX:
   - Batch PS script for chapter iteration (ASR → align tx → refine-sentences → roomtone).
   - Integrate `scripts/RoomtoneCli` into `Ams.Cli audio roomtone` behind `SkipZig=true` gate.
   - `--tone-wav` to loop captured room tone; `--curve equal-power` for content↔content joins.
7) DSP Host: Sketch `IAudioNode` and `TimelineStitcherNode`; prepare for real-time audio processing integration.
8) Doctor output for CI: add `--json` to emit a machine-readable verification report; consider warning categories (apostrophes, hyphenation, page-number runs) with counts.

Risks/Notes
- DOCX style: using `Paragraph.StyleId` to avoid API obsolescence; style/kind are best‑effort.
- Tokenization: whitespace-only preserves punctuation with tokens (e.g., `test.”`), which is intentional for canonical fidelity.
- Determinism: cache JSON settings are stable; ensure environment does not inject non-deterministic timestamps beyond `indexedAt`.
 - Verification scope: `book verify` depends on the canonical schema (`Ams.Core.BookIndex`). If schema evolves, update the doctor to maintain stable hashing and checks.
 - Anchors: relaxation currently ignores `AllowDuplicates` flag and uses separation-based allowance; revisit API if duplicate policy needs to be explicit.
 - Alignment complexity: n-gram indexing is linear in tokens; LIS is `O(k log k)` on candidate anchors `k`—fast for chapter-scale windows.
  - Index spaces: Anchor pipeline/CLI operate in filtered token spaces. Output maps `bp` back to original book `wordIndex` (`bpWordIndex`). ASR timing and original token index are derivable from `AsrResponse.Tokens[ap]` and may be emitted later behind flags.
 - External tools: `aeneas` and `ffmpeg` must be on PATH. On Windows under WSL, normalize `C:\...` → `/mnt/c/...`.
 - Roomtone levels: choose `--tone` (render level) and `--silence-threshold-db` (refine level) per project; defaults are conservative (−60 dBFS tone, −30 dBFS threshold).

Fast Resume Checklist
- Test canonical index: `dotnet run --project host/Ams.Cli -- build-index -b <file.docx> -o <index.json>`; rerun to verify "Found valid cache" and identical JSON bytes.
- Spot-check canonical fidelity: inspect first ~200 tokens for exact text (quotes/dashes/apostrophes preserved).
- Validate script workflow: `validate script` remains unchanged and separate from BookIndex.
- Verify index health: `dotnet run --project host/Ams.Cli -- book verify --index <index.json>`; expect OK + deterministic hash. In CI, non-zero exit signals issues to investigate upstream (tokenization/styles), not to auto-fix.

## Full Pipeline (Fresh Book → Roomtone)

Scope legend
- Per Book: run once per manuscript.
- Per Chapter: run for each chapter WAV (idempotent; can skip when outputs exist).

Per Book
- Build Index: `build-index` → canonical `book-index.json`.
  - Command: `dotnet run --project host/Ams.Cli -- build-index -b <book.docx|.txt|.md|.rtf> -o <book-index.json>`
  - Files: `host/Ams.Cli/Commands/BuildIndexCommand.cs`, `host/Ams.Core/BookParser.cs`, `host/Ams.Core/BookIndexer.cs`.
- Verify (optional): `book verify` to sanity‑check `book-index.json`.
  - Files: `host/Ams.Cli/Commands/BookCommand.cs`.
- Services (once per machine): ensure ASR and Aeneas/FFmpeg are available.
  - ASR (asr-nemo): `services/asr-nemo` (see its README/logs).
  - Aeneas: install Python+aeneas; set env vars if needed:
    - `AENEAS_PYTHON` → python that can run aeneas; `FFMPEG_EXE` → ffmpeg path.

Per Chapter
- ASR: `asr run` → `<chapter>.asr.json`.
  - Command: `dotnet run --project host/Ams.Cli -- asr run -a <chapter.wav> -o <chapter.asr.json> -s <url> -l en`
  - Files: `host/Ams.Cli/Commands/AsrCommand.cs`, `host/Ams.Core/AsrClient.cs`.
- Transcript Index: `align tx` → `<chapter>.tx.json` (sentence/paragraph ScriptRanges bound to tokens).
  - Command: `dotnet run --project host/Ams.Cli -- align tx -i <book-index.json> -j <chapter.asr.json> -a <chapter.wav> -o <chapter.tx.json>`
  - Files: `host/Ams.Cli/Commands/AlignCommand.cs`, `host/Ams.Core/Align/*`, `host/Ams.Core/Align/Tx/TranscriptModels.cs`.
- Sentence Alignment (sentence‑only refine): `refine-sentences` → `<chapter>.sentences.refined.json`.
  - Starts from Aeneas (begin), ends from FFmpeg silencedetect (nearest silence_end after sentence, before next start).
  - Command: `dotnet run --project host/Ams.Cli -- refine-sentences -t <chapter.tx.json> -j <chapter.asr.json> -a <chapter.wav> -o <chapter.sentences.refined.json> --language eng --silence-threshold-db -30 --silence-min-dur 0.12`
  - Files: `host/Ams.Cli/Commands/RefineSentencesCommand.cs`, `host/Ams.Core/SentenceRefinementService.cs`.
- Roomtone Render: fill gaps between sentences; preserve timing; 5 ms crossfades.
  - Command: `dotnet .\scripts\RoomtoneCli\bin\Release\net9.0\RoomtoneCli.dll "<chapter.tx.json>" "<chapter.roomtone.wav>" --sentences "<chapter.sentences.refined.json>" --sr 44100 --fade 5 --tone -60`
  - Files: `scripts/RoomtoneCli/Program.cs`, `scripts/RoomtoneCli/RoomtoneCli.csproj`, `host/Ams.Core/Io/WavIo.cs`.

Batch Orchestration (optional)
- PowerShell script: `scripts/BatchRoomtone.ps1`
  - Loops WAVs (ASR → tx → refine‑sentences → roomtone), supports `-SilenceThresholdDb`, `-SilenceMinDur`, `-Force*` switches, and builds tools (`SkipZig=true`).
  - Typical usage:
    - `./scripts/BatchRoomtone.ps1 -BookIndex <book-index.json> -AudioRoot <folder> -TranscriptionsRoot <folder> -ChapterPattern "*.wav" -AsrService http://localhost:8000 -SilenceThresholdDb -30 -SilenceMinDur 0.12 -SampleRate 44100 -FadeMs 5 -ToneDb -60`

File Index (by function)
- Per Book: `BuildIndexCommand.cs`, `BookParser.cs`, `BookIndexer.cs`, `BookCommand.cs`.
- ASR: `AsrCommand.cs`, `AsrClient.cs`, `services/asr-nemo/*`.
- Align + TX: `AlignCommand.cs`, `Ams.Core/Align/*`, `Align/Tx/TranscriptModels.cs`.
- Sentence refine: `RefineSentencesCommand.cs`, `SentenceRefinementService.cs` (env: `AENEAS_PYTHON`, `FFMPEG_EXE`).
- Roomtone: `scripts/RoomtoneCli/*`, `Ams.Core/Io/WavIo.cs`.
- Batch: `scripts/BatchRoomtone.ps1`.

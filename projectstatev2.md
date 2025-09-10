# ProjectState v2 (Generated)

Date: 2025-09-06
Scope: Single source of truth for the AMS repo’s current state, decisions, and next actions. This document summarizes existing code and adds a concise, action‑oriented plan for the 11‑stage pipeline, DSP tracks, and Zig direction.

Generated vs existing
- Generated: This document and all “Plan/Next” sections.
- Existing: Code and docs referenced by path (e.g., `host/Ams.Core/*`, `ProjectState.md`, `Refactorv2.md`).

---

## Executive Summary
- Product: Audiobook Mastering Suite (AMS) with an idempotent, cacheable 11‑stage pipeline for chapter processing, plus room‑tone stitching and validation gates.
- Current focus: Anchor‑first, windowed alignment with deterministic fingerprints and observable metrics. FFmpeg handles rendering; Zig/DSP is used for demos and will later back real‑time nodes.
- Status (2025‑09‑06): All 11 stages implemented in C# under `host/Ams.Core/Pipeline` and wired by `AsrPipelineRunner` (timeline → validate). CLI verbs exist in `host/Ams.Cli`. Python services (`services/asr-nemo`, `services/aeneas`) present for ASR and forced alignment.
- What’s next: Harden observability + CI gates, finalize comparison‑layer normalization, and add targeted “repair” verb to re‑run failing windows.

---

## Architecture Map (Current)
- `host/Ams.Cli`: CLI entry points (`asr`, `anchors`, `windows`, `window-align`, `refine`, `collate`, `script-compare`, `validate`, helpers under `align/*`).
- `host/Ams.Core`: Pipeline stages, IO, alignment, TX, normalization/comparison, DSP interop scaffolding.
  - Pipeline stages implemented: `DetectSilenceStage`, `PlanWindowsStage`, `ChunkAudioStage`, `TranscribeStage`, `AnchorsStage`, `WindowsStage`, `WindowAlignStage`, `RefineStage`, `CollateStage`, `ScriptCompareStage`, `ValidateStage`.
  - Runner: `AsrPipelineRunner` orchestrates 11 stages; writes `.ams/manifest.json`.
- `host/Ams.Tests`: Unit/integration tests for planner, parser, anchors, TX alignment, refine, pipeline glue.
- `services/aeneas`: FastAPI wrapper for forced alignment.
- `services/asr-nemo`: FastAPI ASR service.
- `scripts/RoomtoneCli`: Offline room‑tone renderer and stitcher (FFmpeg‑based pipeline assistance).
- `dsp`: Zig demo(s) with C ABI and .NET P/Invoke wrapper; not required for collate today.

Reference specs
- Prior state: `ProjectState.md` (2025‑09‑04) — 8‑stage graph, deterministic chunking, sentence‑only refine.
- Current plan/spec: `Refactorv2.md` (2025‑09‑06) — elevates anchors/windows and formalizes the 11‑stage graph with gates and repair.

---

## Stage Graph (v2) — What Exists Today

Order: timeline → plan → chunks → transcripts → anchors → windows → window-align → refine → collate → script-compare → validate

Implementation snapshot
- Implemented (code present under `host/Ams.Core/Pipeline`):
  - `DetectSilenceStage.cs`, `PlanWindowsStage.cs`, `ChunkAudioStage.cs`, `TranscribeStage.cs`, `AnchorsStage.cs`, `WindowsStage.cs`, `WindowAlignStage.cs`, `RefineStage.cs`, `CollateStage.cs`, `ScriptCompareStage.cs`, `ValidateStage.cs`.
- Runner: `AsrPipelineRunner.cs` defines and runs all 11 stages; supports `--from/--to` and `--force` semantics via stage status clearing.
- CLI: Commands exist in `host/Ams.Cli/Commands` (see `AlignCommand.cs` and related commands); verbs match the stage names and helper tools.
- Manifests: `.ams/manifest.json` plus per‑stage `status.json`, `meta.json`, canonical JSON and fingerprints.

Gaps/TODO (short)
- Finalize comparison‑layer normalization rulesets (case/punct/lexicon) and include rule hash in fingerprints and reports.
- Emit per‑window scorecards (`script-compare/map.jsonl`) and seam logs consistently; wire to CI gates.
- Add `repair` verb to re‑run only failing windows (plan exists in `Refactorv2.md`).

---

## How To Run (Minimal, Local)
- Services
  - Start ASR: `uvicorn services.asr-nemo.app:app` (or provided scripts under `services/asr-nemo`).
  - Start Aeneas: `uvicorn services.aeneas.app:app` (ensure `aeneas` + `ffmpeg` installed and on PATH).
- Pipeline (chapter input WAV; authoritative SR 44.1 kHz)
  - Detect: `dotnet run --project host/Ams.Cli -- asr detect-silence --in <audio.wav> --work <audio.wav.ams>`
  - Plan:   `dotnet run --project host/Ams.Cli -- asr plan-windows   --in <audio.wav> --work <audio.wav.ams>`
  - Run E2E: `dotnet run --project host/Ams.Cli -- asr run --in <audio.wav> --work <audio.wav.ams> --from timeline --to validate`
- Utilities
  - Build book index: `dotnet run --project host/Ams.Cli -- build-index -b <book.docx|.txt|.md|.rtf> -o <book.index.json>`
  - Anchors only: `dotnet run --project host/Ams.Cli -- align anchors -i <book.index.json> -j <chapter.asr.json> --emit-windows`
  - TX index: `dotnet run --project host/Ams.Cli -- align tx -i <book.index.json> -j <chapter.asr.json> -a <chapter.wav> -o <chapter.tx.json>`

Notes
- Paths: if services run on Linux and CLI on Windows, normalize `C:\…` → `/mnt/c/...` for service calls.
- Determinism: Prefer CPU for exact reproducibility; otherwise seed GPU and document drift.

---

## Quality Gates & Metrics (v2 Targets)
- Opening window retention (0–10 s): ≥ 0.995
- Seam issues: duplications = 0, omissions = 0
- Short phrase loss rate (≤ 1.2 s): ≤ 0.005
- Anchor coverage: ≥ 0.85; drift p95 ≤ 0.8 s
- WER/CER: project thresholds (set per title); emit chapter + per‑window
- CI: tiny asset runs full pipeline and enforces above gates

---

## Risks & Mitigations
- Sparse anchors in certain prose
  - Mitigate: relax `n`, widen pads, or allow soft anchors via repair plan; add domain stopwords.
- Noisy/slow openings
  - Mitigate: increase `pre_pad_s`, enforce opening‑sentinel rules, and flag in validation.
- Service drift/version skew
  - Mitigate: pin `/v1/version` in fingerprints; fail fast on unknown versions.
- Cross‑platform paths
  - Mitigate: centralized Windows→WSL mapping; never guess inside services.

---

## .NET Systems Track (Short → Mid)
- Short (1–2 weeks)
  - Wire per‑window scorecards and seam logs to CI artifacts.
  - Implement `repair` verb and selective re‑run (windows → window-align → refine → collate → script-compare → validate).
  - Stabilize canonical JSON writer (key order, decimals, no scientific notation) across all stages.
- Mid (3–6 weeks)
  - Desktop runner (Avalonia) to visualize windows, anchors, seams, and failing gates.
  - Project templates and “book profile” config (per‑title thresholds, stopwords, pads).
  - Parallelism controls and back‑pressure for transcripts/window‑align.

---

## DSP Track (Offline First)
- Today
  - Room‑tone stitching via FFmpeg with deterministic joins and zipper hysteresis.
  - Zig gain demo + P/Invoke wrapper exists; not on hot path.
- Next
  - Offline DSP nodes as CLI filters: EQ (RBJ biquads), high‑quality limiter (ITU‑R BS.1770 loudness meter for reporting), de‑clicker prototype.
  - Unit tests with golden WAV fixtures; no allocations/locks inside processing loops.
- Later
  - Reusable realtime‑safe DSP core in Zig with C ABI. Host surface in .NET for node‑graph authoring.

---

## Zig Track (Foundation)
- Near term
  - Solidify C ABI for a few primitives (gain, biquad array, ring buffer), ship `.dll/.so` with deterministic tests.
- Mid term
  - Real‑time constraints: lock‑free queues, fixed‑cap allocators, no syscalls in process loop; integration plan for Avalonia host.

---

## Testing & CI
- Unit: tokenizer, anchors LIS, windows coverage, TX invariants, fingerprint stability, zipper monotonicity.
- Integration: tiny assets for anchors → windows → window‑align → refine; byte‑stable JSON.
- E2E: compose services; enforce gates from Quality section.
- Local command: `dotnet test` under `host/`.

---

## Decision Log (Recent)
- 2025‑09‑06: Adopt 11‑stage graph; anchors/windows first‑class; add opening sentinel and comparison‑layer normalization; introduce `repair` plan.
- 2025‑09‑04: Manifest v2 with fingerprinted, idempotent stages and deterministic chunking.

---

## Immediate Next Actions (Owner → Agent Role)
- Wire map.jsonl and seam logs to CI artifacts — Engineer + Critic
- Implement `ams repair` selective window rerun — Engineer
- Finalize normalization rule set and hash — Architect + Educator
- Add project profile config (thresholds/stopwords) — Architect + Engineer
- Tiny‑asset CI enforcing gates — Engineer

---

## Backlog (Trimmed)
- Avalonia visualizer for windows/anchors/seams.
- Soft‑anchor authoring UX for manual hints.
- DSP: limiter + LUFS meter; EQ blocks; fixture harness.
- Zig: RT buffer toolkit + deterministic test suite.
- Book parser: optional `charStart/charEnd` spans; cache key with parser version.

---

## Pointers
- Specs & plans: `Refactorv2.md`, `ProjectState.md`, `Consolidate.md`
- Key code: `host/Ams.Core/Pipeline/*`, `host/Ams.Cli/Commands/*`, `host/Ams.Core/Align/*`, `host/Ams.Core/Io/WavIo.cs`
- Services: `services/asr-nemo`, `services/aeneas`


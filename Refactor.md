# AMS Refactor Plan

Date: 2025-09-04
Scope: Consolidate functionality in `/host`, remove duplicated steps (e.g., silence detection), and organize a single, deterministic pipeline where each command runs once with clear ownership and artifacts.

## Goals
- Single, end-to-end pipeline with discrete, idempotent stages.
- One source of truth for each concern (detect, plan, chunk, transcribe, align, refine, export).
- Clear CLI verbs that map 1:1 to stages, plus a one-shot orchestrator that respects caching.
- Versioned, reproducible artifacts and a manifest that coordinates all stages.
- Preserve UI functionality while migrating to the new pipeline.

## Repository Snapshot (Relevant to /host)
- `host/Ams.sln` (.NET 9)
- `host/Ams.Core` — domain logic
  - `Asr/Pipeline/`:
    - `FfmpegSilenceDetector.cs` (ISilenceDetector)
    - `SilenceWindowPlanner.cs` (IChunkPlanner)
    - `Models.cs` (SilenceParams, SegmentationParams, ChunkPlan, AsrManifest, etc.)
    - `ProcessRunner.cs` (IProcessRunner)
  - `Io/WavIo.cs`, `Dsp/RoomtoneRenderer.cs`, `Align/*`, `Normalization/*`, etc.
- `host/Ams.Cli` — System.CommandLine CLI
- `Commands/AsrCommand.cs`
- `host/Ams.Dsp.Native` — interop wrapper over native (Zig) DSP
- `host/Ams.UI.Avalonia` — desktop UI
- `host/Ams.Tests` — unit tests (e.g., `SilenceWindowPlannerTests.cs`)
- Services and DSP
  - `services/asr-nemo` — FastAPI-based ASR service consumed via `Ams.Core.AsrClient`
  - `dsp/` — Zig native build outputs used by `Ams.Dsp.Native`

---

## Target Architecture

Ams.Core owns business logic and the pipeline; Ams.Cli orchestrates; UI reads artifacts; ASR is external; Native DSP is wrapped via a stable interface.

```text
Ams.Cli ──commands──> Ams.Core (Pipeline Orchestrator, Manifest SoT)
   │                                    │
   └────── one‑shot & stage verbs        ├── AsrClient ──HTTP──> services/asr-nemo
                                         ├── IO/DSP/Align/Tx
                                         └── IDspKernel ───────> Ams.Dsp.Native (Zig)
Ams.UI.Avalonia <──reads manifest/artifacts──┘
```

### Responsibilities
- Ams.Core: `ISilenceDetector`, `IChunkPlanner`, `IProcessRunner`, `IAsrClient`; pipeline orchestration; artifact schemas.
- Ams.Cli: thin command surface over Core; parameters and file layout only.
- Ams.Dsp.Native: heavy kernels behind `IDspKernel` (defined in Core); no duplicated algorithms in Core.
- Ams.UI.Avalonia: reads `manifest.json` and stage artifacts for progress/results; does not execute business logic.

---

## Canonical Pipeline (Stages, Artifacts, Idempotency)
Work directory per input (default): `<input>.ams/`. Every stage writes its primary artifacts plus `params.snapshot.json` and `meta.json` (tool versions, git sha).

| Stage | Purpose | Inputs | Outputs (canonical) | Idempotency (skip rule) | Interfaces |
|---|---|---|---|---|---|
| init | Create workdir, hash input, seed manifest | `input.wav` | `manifest.json` | If `manifest.json` exists with same input hash | — |
| detect-silence | Find candidate silence windows | `input.wav`, `params.silence.json` | `timeline/silence.json` | Same input hash + params + ffmpeg version | `ISilenceDetector`, `IProcessRunner` |
| plan-windows | DP segmentation using silence midpoints | `timeline/silence.json`, `params.plan.json` | `plan/windows.json` | Same inputs + params | `IChunkPlanner` (SilenceWindowPlanner) |
| chunk-audio | Export chunk WAVs per window | `input.wav`, `plan/windows.json`, `params.chunk.json` | `chunks/*.wav`, `chunks/index.json` | Same inputs; verifies chunk hashes | `IProcessRunner` (ffmpeg split) |
| transcribe | ASR per chunk | `chunks/*.wav`, `params.asr.json` | `transcripts/*.json`, `transcripts/metrics.json` | Same chunk hashes + model/params | `AsrClient` |
| collate | Merge chunk transcripts | `transcripts/*.json` | `transcript.json` | Same transcripts | Core |
| align | Align transcript to anchors/index | `transcript.json`, optional book index | `align/aligned.json` | Same transcript + params | Align core |
| refine | Sentence/paragraph refinement | `align/aligned.json`, `params.refine.json` | `refine/paragraphs.json`, `refine/notes.json` | Same aligned + params | Text core |
| export | Output final formats | `refine/paragraphs.json`, `params.export.json` | `export/*.srt|*.txt|*.json` | Same inputs + template | Exporters |
| validate | Cross-artifact checks | `manifest.json`, artifacts | `validate/report.json` | Always recompute (read-only) | Validators |

Key rule: Each stage runs at most once for a given fingerprint (inputs + params + tool versions). Re-running with `--resume` skips; `--force` invalidates.

---

## De-duplication & Sources of Truth
- Silence detection runs only in `detect-silence` via `ISilenceDetector` (ffmpeg). No implicit re-detection in planner or elsewhere.
- Segmentation exists only in `plan-windows` via `IChunkPlanner` (deterministic DP). Tail relaxation and constraints are param-driven.
- Chunk splitting only in `chunk-audio`; no stage performs hidden splitting.
- Transcription only through `AsrClient` over chunk WAVs; UI does not invoke ASR directly.
- DSP algorithms centralized: Core hosts composition utilities; heavy kernels live behind `IDspKernel` implemented by `Ams.Dsp.Native`.

---

## CLI Map (Single-Purpose Verbs + Orchestrator)
Conventions: `--in <file.wav|dir>`, `--work <dir>` (default `<input>.ams`), `--params <file.json>`, `--resume`, `--force`, `--from/--to`, `--jobs N`.

- `ams asr detect-silence --in book.wav [--work book.wav.ams] [--params silence.json]`
- `ams asr plan-windows   --in book.wav [--work book.wav.ams] [--params plan.json]`
- `ams asr chunk-audio    --in book.wav [--work book.wav.ams]`
- `ams asr transcribe     --in book.wav [--work book.wav.ams] [--jobs 4]`
- `ams asr collate        --in book.wav [--work book.wav.ams]`
- `ams asr align          --in book.wav [--work book.wav.ams]`
- `ams asr refine         --in book.wav [--work book.wav.ams]`
- `ams asr export         --in book.wav [--work book.wav.ams] --format srt,txt`
- `ams asr run            --in book.wav [--work book.wav.ams] [--from detect-silence] [--to export] [--resume|--force] [--jobs 4]`
- `ams validate           --work book.wav.ams`
- `ams audio roomtone     --in input.wav --out output.wav [--params dsp.json]` (optional DSP utility)

Examples:
```bash
# End-to-end with caching
ams asr run --in book.wav --work book.wav.ams --jobs 4 --resume

# Re-plan only with different constraints
ams asr plan-windows --in book.wav --work book.wav.ams --params plan.tight.json --force

# Export results without recomputation
ams asr export --in book.wav --work book.wav.ams --format srt,txt --resume
```

---

## Artifacts & Conventions
- Work root: `<input>.ams/`
- Manifest: `manifest.json` (schema: `asr-manifest/v2`) records input hash, stage status, artifacts, params fingerprints, tool versions, errors.
- Per-stage dirs: `timeline/`, `plan/`, `chunks/`, `transcripts/`, `align/`, `refine/`, `export/`, `validate/`.
- Every stage writes:
  - Primary artifact(s)
  - `params.snapshot.json` — fully-resolved effective params
  - `meta.json` — tool versions (ffmpeg, ASR model tag), OS info, git sha
  - `status.json` — start/end timestamps, duration, attempts

Minimal manifest sketch:
```json
{
  "schema": "asr-manifest/v2",
  "input": { "path": ".../book.wav", "sha256": "...", "duration_sec": 1234.56 },
  "stages": {
    "detect-silence": {
      "status": "completed",
      "artifacts": { "timeline": "timeline/silence.json" },
      "fingerprint": { "input": "...", "params": "...", "tool": { "ffmpeg": "6.1.1" } },
      "started": "...", "ended": "...", "attempts": 1
    }
  }
}
```

---

## Migration Plan (Phased)
**Phase 0 — Inventory & Freeze (S)**
- Document current behaviors and artifacts; avoid adding new implicit calls.
- ADR: “Single-pipeline, artifact-first orchestration.”

**Phase 1 — Manifest v2 & Layout (M)**
- Introduce `asr-manifest/v2` + JSON Schema; add adapter to read v1 artifacts into v2.
- Canonical stage directories and filenames; shims for legacy names.

**Phase 2 — Silence Detection De-dup (S–M)**
- Ensure `plan-windows` only consumes `timeline/silence.json` (no implicit detection).
- `Ams.Cli` routes `asr detect-silence` exclusively through `ISilenceDetector`.

**Phase 3 — Planner Consolidation (S)**
- All segmentation via `IChunkPlanner` (SilenceWindowPlanner); add fingerprints and determinism checks.

**Phase 4 — Chunking Stage (M)**
- Implement `chunk-audio` + `chunks/index.json` with per-chunk hash/duration.
- Integration tests with tiny WAVs.

**Phase 5 — Orchestrator (M)**
- `asr run` with `--from/--to`, `--resume/--force`, `--jobs` parallelism; skip-on-up-to-date.

**Phase 6 — UI Integration (M)**
- UI reads v2 manifest; progress from `status.json`; maintain old CLI aliases with deprecation notices.

**Phase 7 — DSP Boundary Cleanup (M–L)**
- Define `IDspKernel` in Core; move heavy kernels to Native; unify DSP utilities; add perf/interop tests.

**Phase 8 — Cleanup & Docs (S)**
- Remove deprecated commands/paths; finalize docs; ship `validate` rules.

Acceptance per phase: green unit/integration tests, reproducible artifacts, updated docs, demo script for a 1‑minute WAV.

---

## Testing & Observability
- Unit tests: planner invariants (min/max/target, tail relaxation), `WavIo` duration accuracy, manifest fingerprinting, ffmpeg parser.
- Integration tests: detect→plan→chunk (no ASR); transcribe with mocked `AsrClient`; optional e2e job against dev ASR.
- Golden artifacts: freeze `silence.json`, `windows.json`, `chunks/index.json` for fixtures; assert with pinned tool versions.
- Logs & metrics: structured logs (Serilog), stage timings, cache hits/misses, ASR latency p50/p95; write `metrics.json` per stage as needed.

---

## Governance
- Coding standards: .NET 9; nullable enabled; records for artifact models; analyzers in CI.
- Versioning: schema SemVer; bump minor for additive, major for breaking; cache invalidation on tool version changes (ffmpeg, ASR model).
- Feature flags: `ASR_ORCH_V2=1` to enable orchestrator during migration; planner strategy tag (`dp_v1`, `dp_v1_relaxed`).
- Service hardening: timeouts/retries/backoff in `AsrClient`; `/health` check before transcribe.

---

## Risks & Mitigations
- Artifact drift breaking legacy runs → versioned schema + adapters; `validate` emits actionable guidance.
- Cache poisoning (stale inputs) → fingerprints over content, params, and tool versions; mandatory checks.
- FFmpeg variance → record version; allow pinning; re-run detection if version changes.
- ASR model drift → record model tag/commit; invalidate transcribe on change.
- Parallelism overload → cap `--jobs`; resource-aware scheduling.
- UI/CLI mismatch mid-migration → keep aliases; feature flag; compatibility layer in UI.
- Native DSP ABI changes → versioned P/Invoke wrapper; interop tests.

---

## Backwards Compatibility (Ams.UI.Avalonia)
- Short term: Manifest reader supporting v1 and v2; file watchers for `manifest.json` and per‑stage `status.json`.
- Medium term: UI triggers `asr run` with stage bounds; parameters saved to `params.*.json`; shows cache state (Up‑to‑date/Dirty/Error).
- Migration UX: “Import legacy run” that indexes existing outputs into v2 manifest without copying data.

---

## Immediate Next Steps (1–2 days)
- Draft ADR and JSON Schema for `asr-manifest/v2` and stage directories.
- Scaffold `Ams.Core.Pipeline` (`StageRunner` base, `AsrPipelineRunner`, fingerprinting utilities).
- Wire existing `asr silence` and `asr plan` verbs to the orchestrator contracts (no behavior change yet).
- Add `ams validate` to surface schema/tool/version issues early.

---

## Appendix: Workdir Layout
```text
book.wav.ams/
  manifest.json
  timeline/
    silence.json
    params.snapshot.json
    meta.json
    status.json
  plan/
    windows.json
    params.snapshot.json
    meta.json
    status.json
  chunks/
    index.json
    0001.wav
    0002.wav
    ...
  transcripts/
    0001.json
    metrics.json
  align/
    aligned.json
  refine/
    paragraphs.json
  export/
    book.srt
    book.txt
  validate/
    report.json
```

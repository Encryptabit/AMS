# Changelog

All notable changes to this project will be documented in this file.

## 2025-09-04 — Stage‑Oriented ASR Pipeline (Manifest v2, New CLI Verbs, Idempotency)

- Summary: Introduced an idempotent, artifact‑first ASR pipeline with a versioned manifest and new stage‑oriented CLI.

- Added
  - Core: `Ams.Core/Pipeline/{StageRunner, AsrPipelineRunner}`.
  - Stages: `DetectSilenceStage` → `timeline/silence.json`; `PlanWindowsStage` → `plan/windows.json`.
  - CLI: `asr detect-silence`, `asr plan-windows`, `asr run` (orchestrator), `validate`.
  - Manifest v2 (schema: `asr-manifest/v2`) embedded in `Ams.Core/Asr/Pipeline/Models.cs`.
  - Pipeline integration tests (mock ffmpeg). Total tests now 42.

- Changed
  - `FfmpegSilenceDetector.ParseVersion` made `public` for tool‑version fingerprinting.
  - `AsrCommand` wired to new stage verbs; `Program` exposes `validate` manifest command.

- Deprecated
  - `asr silence` and `asr plan` remain functional but print deprecation notices. Prefer `detect-silence` and `plan-windows`.

- Notes
  - Workdir convention: `<input>.ams/` with per‑stage `params.snapshot.json`, `meta.json`, `status.json`.
  - Idempotency: stages skip when fingerprints (input, params, tool versions) match; `--force` invalidates from a chosen stage.

- Migration
  - Continue using legacy verbs if needed; begin adopting new stage verbs and orchestrator.
  - UI can read v2 manifest; legacy flows unaffected.


# AMS Core Slice Alignment Plans

Last updated: 2026-05-16

Reader: an engineer preparing or reviewing an AMS Core slice cleanup.

Post-read action: open the owning slice doc before changing code, then update the concrete alignment plan, decisions, code sketches, and open questions for that slice.

## Purpose

The feature slice catalogue answers "where does this code belong?" The refactor northstar answers "what standard should the cleanup meet?" These slice docs answer "what concrete changes does this slice need in order to meet that standard?"

Use a slice doc for:

- specific changes needed to align the slice to the northstar;
- slice-specific decisions;
- code sketches that would make the northstar too busy;
- boundary notes between slices;
- dead-code or one-off cleanup findings;
- open audit questions that should not be lost.

Do not put long implementation sketches in the northstar. If a decision affects more than one slice, record it in the primary owning slice and add a short cross-slice note in the other affected slice.

## Slice Docs

| Slice | Alignment plan | Responsibility |
|---|---|---|
| FS00 | [Build and Project Surface](fs00-build-project-surface.md) | Project build rules, assembly metadata, global compilation context |
| FS01 | [Runtime Workspace and Artifact Lifecycle](fs01-runtime-workspace-artifact-lifecycle.md) | Workspace abstraction, book/chapter/audio context lifecycles, artifact path resolution, lazy document slots |
| FS02 | [Book Ingestion, Indexing, and Pronunciation](fs02-book-ingestion-indexing-pronunciation.md) | Manuscript parsing, book indexing, cache, pronunciation lookup, proper noun prompting |
| FS03 | [ASR Transcription](fs03-asr-transcription.md) | ASR engine selection, Whisper.NET processing, ASR service contracts, transcript response models |
| FS04 | [Audio DSP, QC, and FFmpeg Integration](fs04-audio-dsp-qc-ffmpeg.md) | Audio buffers through treatment, splicing, QC, silence detection, FFmpeg wrappers, filter specs |
| FS05 | [Alignment, Timing, and Artifact Contracts](fs05-alignment-timing-artifact-contracts.md) | Anchor selection, transcript alignment, hydration, MFA timing merge models, timing DTOs |
| FS06 | [MFA Forced Alignment](fs06-mfa-forced-alignment.md) | MFA corpus construction, G2P/pronunciation support, process invocation, TextGrid aggregation |
| FS07 | [Use-Case Commands and Pipeline Entry Points](fs07-use-case-commands-pipeline-entry-points.md) | Command wrappers, pipeline orchestration, run states, module IDs, progress/failure contracts |
| FS08 | [Benchmark and Determinism](fs08-benchmark-determinism.md) | Benchmark run/compare contracts, deterministic gate, metrics, manifests, artifact store |
| FS09 | [Validation and Reporting](fs09-validation-reporting.md) | Validation reports, script validation, hydrated/text diff scoring |
| FS10 | [Prosody and Pause Dynamics](fs10-prosody-pause-dynamics.md) | Pause maps, pause policies, dynamics/compression math, timeline application |
| FS11 | [Common Infrastructure](fs11-common-infrastructure.md) | Logging, path resolution, text normalization, natural sorting, edit distance helpers |
| FS12 | [Embedded Resources and Model Assets](fs12-embedded-resources-model-assets.md) | Embedded word-frequency resource and bundled FFmpeg/Tesseract/Silero assets |

# AMS Core Layout

## Goals
- Keep the existing CLI verbs and artifact formats stable while we reorganize the code.
- Group related services and models under domain-focused namespaces to avoid duplication.
- Prepare the codebase for mandatory ASR timing metadata and the upcoming room-tone cleanup stage.

## Domain Namespaces

### `Ams.Core.Runtime.Documents`
- Book parsing, indexing, caching, and related models.
- Responsible for emitting `book-index.json` and validating source manuscripts.

### `Ams.Core.Alignment`
- Anchor discovery, transcript alignment, section detection, and window building.
- Produces `*.align.tx.json`, `*.anchors.json`, and the hydrated transcript artifacts.

### `Ams.Core.Asr`
- ASR client integrations, transcript merge logic, and pipeline helpers.
- Communicates with the external FastAPI ASR service and manages transcription metadata.

### `Ams.Core.Audio`
- DSP utilities, audio analysis, and room-tone rendering.
- Hosts `AudioAnalysisService` and upcoming sentence-level RMS heuristics.

### `Ams.Core.Pipeline`
- Stage orchestration, manifest handling, and CLI-facing workflow glue.
- Legacy stages remain alongside new room-tone stages for side-by-side validation.

### `Ams.Core.Artifacts`
- DTOs and serializers shared across stages (e.g., `TranscriptIndex`, `SentenceAlign`).
- Encapsulates the canonical JSON shapes consumed by CLI tooling.

### `Ams.Core.Diagnostics`
- Fingerprint computation, logging helpers, and telemetry emitters.

### `Ams.Core.Common`
- Cross-cutting utilities (text normalization, precision helpers, constants).
- Aimed at small, dependency-light helpers to keep other namespaces clean.

## Timing Model
- Introduce `TimingRange(double StartSec, double EndSec)` as the canonical container for timing spans.
- `FragmentTiming` will derive from `TimingRange` and add chunk provenance fields.
- Sentence-level models (e.g., `SentenceAlign`, refinement outputs) extend or wrap `TimingRange` to stay consistent.

## Artifact Contracts (unchanged)
- `book-index.json`: Canonical manuscript index (words, sentences, sections). No schema changes.
- `*.anchors.json`: Anchor selections plus policy metadata. Section detection remains deterministic.
- `*.align.tx.json`: Transcript index enriched with mandatory ASR timing (StartSec/EndSec) per sentence.
- `*.align.hydrate.json`: Hydrated transcript with word-level aligns and timing metadata.
- Room-tone stage will output `cleanup/meta.json`, `cleanup/params.snapshot.json`, and a processed WAV.

## Migration Phases
1. **Baseline** – capture hashes for existing artifacts under `analysis/` and record CLI commands.
2. **Architecture Prep** – publish this document, add namespace placeholders, and enable dependency linting.
3. **Domain Moves** – relocate files into the new namespaces folder-by-folder, verifying artifacts after each move.
4. **Timing Enrichment** – extend alignment outputs with required `TimingRange` data and update tests.
5. **Audio & Room-Tone** – enhance audio analysis, build the sentence timeline, and add the new cleanup stage.

## Verification
- `dotnet build -c Release`
- `dotnet test -c Release`
- Existing Chapter 1 pipeline (ASR ? align) must produce byte-identical artifacts until Phase 4 introduces new files.
- Post Phase 4, inspect room-tone outputs manually and log timing QA metrics for each run.

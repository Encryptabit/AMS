# Phase 14 Context: Shared Chunk Plan for ASR + MFA

## Decisions (Locked)

- Keep AMS artifact architecture consistent with existing patterns:
  - artifacts are represented as first-class document models
  - chapter-scoped persistence goes through `IArtifactResolver`
  - chapter runtime state is accessed through `ChapterDocuments` `DocumentSlot<T>` slots
  - audio loading continues through `AudioBufferManager` / `AudioBufferContext`

- Keep lexical transcript truth for MFA corpus generation sourced from book-side text (`HydratedSentence.BookText`), not raw ASR transcript words.

- Shared chunking should drive both ASR and MFA so chunk boundaries are deterministic and reusable across stages.

- ASR chunking remains non-overlapping by default to avoid duplicate boundary tokens in merged ASR output.

- MFA may use context expansion at chunk boundaries when needed for robust lab generation, but this must not alter ASR chunk semantics.

- Preserve existing canonical chapter artifact paths and downstream contracts:
  - `TextGrid` remains consumable by existing `MergeTimingsCommand`
  - transcript/hydrate/anchors/asr artifact names remain stable

## Claude's Discretion

- Exact shape of the chunk plan document schema (fields beyond core timing ids)
- Service placement (`Ams.Core.Services.Alignment` vs adjacent namespace)
- Optional feature flags and defaults for incremental rollout
- Test fixture strategy and benchmark dataset choice

## Deferred Ideas (Out of Scope for This Phase)

- Full storage migration from JSON artifacts to SQLite
- External package extraction for MFA runtime (`Mfa.Net`)
- UI redesign work unrelated to chunk planning and MFA/ASR execution path

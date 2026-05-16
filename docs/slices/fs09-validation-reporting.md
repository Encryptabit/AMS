# FS09: Validation And Reporting

Last updated: 2026-05-16

Reader: an engineer changing validation reports, script validation, report models, or hydrated/text diff scoring.

Post-read action: record specific FS09 alignment work here before changing validation failure shapes, report DTOs, or diff scoring behavior.

## Scope

FS09 owns validation reports, script validation, hydrated/text diff scoring, and report view models.

## Current Concepts

- Script validation compares audio/ASR/book-derived data.
- Report builders transform validation results into report views.
- Text diff analysis computes scoring details and coverage.

## Specific Changes Needed

No slice-specific changes recorded yet.

## Decisions

No slice-specific decisions recorded yet.

## Code Sketches

No code sketches recorded yet.

## Open Audit Questions

- Which validation failures are user-facing findings versus internal errors?
- Which report DTOs need construction guards?
- Which diff-scoring behavior needs old-output oracle tests before cleanup?

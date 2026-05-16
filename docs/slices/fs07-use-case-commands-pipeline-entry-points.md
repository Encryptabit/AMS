# FS07: Use-Case Commands And Pipeline Entry Points

Last updated: 2026-05-16

Reader: an engineer changing command wrappers, pipeline orchestration, run state, progress, or failure contracts.

Post-read action: record specific FS07 alignment work here before changing orchestration boundaries, pipeline stages, module IDs, progress updates, or run failure shapes.

## Scope

FS07 owns command wrappers, pipeline orchestration, run states, module IDs, progress/failure contracts, and application entry points.

## Current Concepts

- Commands wrap individual application use cases.
- Pipeline orchestration coordinates stage execution.
- Run state, module IDs, progress updates, artifacts, and failures describe execution.
- Recovery tiers and concurrency controls shape operating behavior.

## Specific Changes Needed

No slice-specific changes recorded yet.

## Decisions

No slice-specific decisions recorded yet.

## Code Sketches

No code sketches recorded yet.

## Open Audit Questions

- Which failures should become `RunFailure` versus exceptions?
- Which stage transitions need explicit state types?
- Which pipeline artifacts need stronger construction rules?

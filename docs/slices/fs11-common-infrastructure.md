# FS11: Common Infrastructure

Last updated: 2026-05-16

Reader: an engineer changing logging, path resolution, text normalization, natural sorting, or edit-distance helpers.

Post-read action: record specific FS11 alignment work here before changing shared infrastructure behavior that many slices depend on.

## Scope

FS11 owns logging, path resolution, text normalization, natural sorting, and edit distance helpers.

## Current Concepts

- Logging provides shared diagnostic output.
- Path resolution translates across platform/path conventions.
- Text normalization and natural sorting support book, alignment, and reporting workflows.
- Edit-distance helpers support scoring and comparison.

## Specific Changes Needed

No slice-specific changes recorded yet.

## Decisions

No slice-specific decisions recorded yet.

## Code Sketches

No code sketches recorded yet.

## Open Audit Questions

- Which path resolution rules should move into FS01 artifact address concepts?
- Which normalizers have domain-specific behavior that should move closer to the owning slice?
- Which helper APIs need narrower names to avoid misuse?

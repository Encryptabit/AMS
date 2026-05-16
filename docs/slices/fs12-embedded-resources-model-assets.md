# FS12: Embedded Resources And Model Assets

Last updated: 2026-05-16

Reader: an engineer changing embedded resources, bundled models, or asset packaging.

Post-read action: record specific FS12 alignment work here before changing bundled resources, model assets, or copy/deployment expectations.

## Scope

FS12 owns embedded word-frequency resources and bundled FFmpeg/Tesseract/Silero assets.

## Current Concepts

- Embedded text resources support Core lookup behavior.
- Bundled binary/model assets support local audio and OCR-related processing.
- Project build rules decide how assets are copied and deployed.

## Specific Changes Needed

No slice-specific changes recorded yet.

## Decisions

No slice-specific decisions recorded yet.

## Code Sketches

No code sketches recorded yet.

## Open Audit Questions

- Which assets are still actively used?
- Which bundled model versions need provenance or checksum documentation?
- Which asset copy rules belong in FS00 versus FS12 documentation?

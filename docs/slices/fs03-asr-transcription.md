# FS03: ASR Transcription

Last updated: 2026-05-16

Reader: an engineer changing ASR engine selection, transcript generation, or ASR service contracts.

Post-read action: record specific FS03 alignment work here before changing engine selection, model selection, prompt filtering, token timing, or transcript generation behavior.

## Scope

FS03 owns speech-to-text engine selection and transcript generation.

## Current Concepts

- ASR engine and model values resolve runtime mode/model.
- ASR processing contains the active Whisper.NET implementation.
- ASR service bridges runtime chapters/audio buffers to ASR artifacts.
- ASR audio preparation normalizes buffers for recognition.

## Specific Changes Needed

No slice-specific changes recorded yet.

## Decisions

No slice-specific decisions recorded yet.

## Code Sketches

No code sketches recorded yet.

## Open Audit Questions

- Which ASR engine/model choices should become validated values?
- Which transcript timing corrections are ASR-owned versus alignment-owned?
- What old behavior should act as oracle before refactoring token splicing or prompt filtering?

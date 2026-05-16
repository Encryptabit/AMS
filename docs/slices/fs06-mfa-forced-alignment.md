# FS06: MFA Forced Alignment

Last updated: 2026-05-16

Reader: an engineer changing MFA corpus construction, pronunciation generation, TextGrid aggregation, or MFA process invocation.

Post-read action: record specific FS06 alignment work here before changing forced-alignment inputs, process boundaries, pronunciation cache behavior, or TextGrid output handling.

## Scope

FS06 owns forced-alignment preparation and invocation.

## Current Concepts

- Chunk corpus builder creates deterministic MFA input files.
- MFA services and process supervisors invoke external MFA/G2P tools.
- Pronunciation providers and lexicon cache support generated pronunciations.
- TextGrid aggregation combines alignment outputs.

## Specific Changes Needed

No slice-specific changes recorded yet.

## Decisions

No slice-specific decisions recorded yet.

## Code Sketches

No code sketches recorded yet.

## Open Audit Questions

- Which process failures are dependency failures versus validation failures?
- Which MFA workspace limits should be explicit?
- Which pronunciation cache invariants should be protected at construction?

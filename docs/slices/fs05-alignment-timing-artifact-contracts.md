# FS05: Alignment, Timing, And Artifact Contracts

Last updated: 2026-05-16

Reader: an engineer changing alignment, timing merge, hydration, or serialized timing/artifact contracts.

Post-read action: record specific FS05 alignment work here before changing anchor selection, transcript alignment, hydration, timing DTOs, or artifact compatibility rules.

## Scope

FS05 owns anchor discovery, transcript alignment, hydration, timing merge, and the JSON DTOs consumed by pipeline stages and hosts.

## Current Concepts

- Anchor processors find stable book/ASR sync points.
- Transcript alignment maps ASR tokens back to book text.
- Hydration enriches transcript indexes with word-level and diff data.
- Artifact records define canonical persisted shapes.

## Specific Changes Needed

No slice-specific changes recorded yet.

## Decisions

No slice-specific decisions recorded yet.

## Code Sketches

No code sketches recorded yet.

## Open Audit Questions

- Which artifact records need explicit compatibility/version rules?
- Which nullable timing ranges represent real absence versus incomplete modeling?
- Which alignment decisions should be tested against old behavior before refactor?

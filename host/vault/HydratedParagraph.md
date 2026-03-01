---
namespace: "Ams.Core.Artifacts.Hydrate"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Artifacts/Hydrate/HydratedTranscript.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Artifacts.Hydrate.HydratedParagraph>"
member_count: 1
dependency_count: 3
pattern: ~
tags:
  - class
---

# HydratedParagraph

> Record in `Ams.Core.Artifacts.Hydrate`

**Path**: `Projects/AMS/host/Ams.Core/Artifacts/Hydrate/HydratedTranscript.cs`

**Implements**:
- IEquatable

## Dependencies
- [[HydratedRange]] (`BookRange`)
- [[ParagraphMetrics]] (`Metrics`)
- [[Ams.Core.Artifacts.Hydrate.HydratedDiff_]] (`Diff`)

## Properties
- `Id`: int
- `BookRange`: HydratedRange
- `SentenceIds`: IReadOnlyList<int>
- `BookText`: string
- `Metrics`: ParagraphMetrics
- `Status`: string
- `Diff`: HydratedDiff?

## Members


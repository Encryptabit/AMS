---
namespace: "Ams.Core.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Artifacts/TranscriptModels.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Artifacts.ParagraphAlign>"
member_count: 1
dependency_count: 2
pattern: ~
tags:
  - class
---

# ParagraphAlign

> Record in `Ams.Core.Artifacts`

**Path**: `Projects/AMS/host/Ams.Core/Artifacts/TranscriptModels.cs`

**Implements**:
- IEquatable

## Dependencies
- [[IntRange]] (`BookRange`)
- [[ParagraphMetrics]] (`Metrics`)

## Properties
- `Id`: int
- `BookRange`: IntRange
- `SentenceIds`: IReadOnlyList<int>
- `Metrics`: ParagraphMetrics
- `Status`: string

## Members


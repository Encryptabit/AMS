---
namespace: "Ams.Core.Processors.Diffing"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Processors.Diffing.TextDiffResult>"
member_count: 1
dependency_count: 2
pattern: ~
tags:
  - class
---

# TextDiffResult

> Record in `Ams.Core.Processors.Diffing`

**Path**: `Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs`

**Implements**:
- IEquatable

## Dependencies
- [[SentenceMetrics]] (`Metrics`)
- [[HydratedDiff]] (`Diff`)

## Properties
- `Metrics`: SentenceMetrics
- `Diff`: HydratedDiff
- `Coverage`: double

## Members


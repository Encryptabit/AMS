---
namespace: "Ams.Core.Application.Validation.Models"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Validation/Models/ValidationReportModels.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Application.Validation.Models.SentenceView>"
member_count: 1
dependency_count: 3
pattern: ~
tags:
  - class
---

# SentenceView

> Record in `Ams.Core.Application.Validation.Models`

**Path**: `Projects/AMS/host/Ams.Core/Application/Validation/Models/ValidationReportModels.cs`

**Implements**:
- IEquatable

## Dependencies
- [[SentenceMetrics]] (`Metrics`)
- [[Ams.Core.Artifacts.TimingRange_]] (`Timing`)
- [[Ams.Core.Artifacts.Hydrate.HydratedDiff_]] (`Diff`)

## Properties
- `Id`: int
- `BookRange`: (int Start, int End)
- `ScriptRange`: (int? Start, int? End)?
- `Metrics`: SentenceMetrics
- `Status`: string
- `BookText`: string?
- `ScriptText`: string?
- `Timing`: TimingRange?
- `Diff`: HydratedDiff?

## Members


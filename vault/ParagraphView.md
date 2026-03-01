---
namespace: "Ams.Core.Application.Validation.Models"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Validation/Models/ValidationReportModels.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Application.Validation.Models.ParagraphView>"
member_count: 1
dependency_count: 2
pattern: ~
tags:
  - class
---

# ParagraphView

> Record in `Ams.Core.Application.Validation.Models`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Validation/Models/ValidationReportModels.cs`

**Implements**:
- IEquatable

## Dependencies
- [[ParagraphMetrics]] (`Metrics`)
- [[Ams.Core.Artifacts.Hydrate.HydratedDiff_]] (`Diff`)

## Properties
- `Id`: int
- `BookRange`: (int Start, int End)
- `Metrics`: ParagraphMetrics
- `Status`: string
- `BookText`: string?
- `Diff`: HydratedDiff?

## Members


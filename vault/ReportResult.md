---
namespace: "Ams.Core.Application.Validation.Models"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Validation/Models/ValidationReportModels.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Application.Validation.Models.ReportResult>"
member_count: 1
dependency_count: 1
pattern: ~
tags:
  - class
---

# ReportResult

> Record in `Ams.Core.Application.Validation.Models`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Validation/Models/ValidationReportModels.cs`

**Implements**:
- IEquatable

## Dependencies
- [[Ams.Core.Application.Validation.Models.WordTallies_]] (`WordTallies`)

## Properties
- `Report`: string
- `Sentences`: IReadOnlyList<SentenceView>
- `Paragraphs`: IReadOnlyList<ParagraphView>
- `WordTallies`: WordTallies?

## Members


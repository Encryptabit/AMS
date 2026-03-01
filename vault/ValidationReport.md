---
namespace: "Ams.Core.Application.Validation.Models"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Validation/Models/ValidationModels.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Application.Validation.Models.ValidationReport>"
member_count: 1
dependency_count: 0
pattern: ~
tags:
  - class
---

# ValidationReport

> Record in `Ams.Core.Application.Validation.Models`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Validation/Models/ValidationModels.cs`

**Implements**:
- IEquatable

## Properties
- `AudioFile`: string
- `ScriptFile`: string
- `AsrFile`: string
- `Timestamp`: DateTime
- `WordErrorRate`: double
- `CharacterErrorRate`: double
- `TotalWords`: int
- `CorrectWords`: int
- `Substitutions`: int
- `Insertions`: int
- `Deletions`: int
- `Findings`: ValidationFinding[]
- `SegmentStats`: SegmentStats[]

## Members


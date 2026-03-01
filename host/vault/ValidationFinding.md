---
namespace: "Ams.Core.Application.Validation.Models"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Validation/Models/ValidationModels.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Application.Validation.Models.ValidationFinding>"
member_count: 1
dependency_count: 2
pattern: ~
tags:
  - class
---

# ValidationFinding

> Record in `Ams.Core.Application.Validation.Models`

**Path**: `Projects/AMS/host/Ams.Core/Application/Validation/Models/ValidationModels.cs`

**Implements**:
- IEquatable

## Dependencies
- [[Ams.Core.Application.Validation.Models.FindingType]] (`Type`)
- [[Ams.Core.Application.Validation.Models.ValidationLevel]] (`Level`)

## Properties
- `Type`: FindingType
- `Level`: ValidationLevel
- `StartTime`: double?
- `EndTime`: double?
- `Expected`: string?
- `Actual`: string?
- `Cost`: double
- `Context`: string?

## Members


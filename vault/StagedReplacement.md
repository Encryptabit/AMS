---
namespace: "Ams.Workstation.Server.Models"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Models/PolishModels.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Workstation.Server.Models.StagedReplacement>"
member_count: 1
dependency_count: 1
pattern: ~
tags:
  - class
---

# StagedReplacement

> Record in `Ams.Workstation.Server.Models`

**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Models/PolishModels.cs`

**Implements**:
- IEquatable

## Dependencies
- [[Ams.Workstation.Server.Models.ReplacementStatus]] (`Status`)

## Properties
- `Id`: string
- `ChapterStem`: string
- `SentenceId`: int
- `OriginalStartSec`: double
- `OriginalEndSec`: double
- `PickupSourcePath`: string
- `PickupStartSec`: double
- `PickupEndSec`: double
- `CrossfadeDurationSec`: double
- `CrossfadeCurve`: string
- `StagedAtUtc`: DateTime
- `Status`: ReplacementStatus

## Members


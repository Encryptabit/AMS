---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseAdjustmentsDocument.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Prosody.PauseAdjustmentsDocument>"
member_count: 4
dependency_count: 1
pattern: ~
tags:
  - class
---

# PauseAdjustmentsDocument

> Record in `Ams.Core.Prosody`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseAdjustmentsDocument.cs`

**Implements**:
- IEquatable

## Dependencies
- [[PausePolicySnapshot]] (`Policy`)

## Properties
- `SourceTranscript`: string
- `GeneratedAtUtc`: DateTime
- `Policy`: PausePolicySnapshot
- `Adjustments`: IReadOnlyList<PauseAdjust>
- `JsonOptions`: JsonSerializerOptions

## Members
- [[PauseAdjustmentsDocument.Load]]
- [[PauseAdjustmentsDocument.Create]]
- [[PauseAdjustmentsDocument.Save]]


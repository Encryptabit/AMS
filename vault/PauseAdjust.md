---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseModels.cs"
access_modifier: "public"
base_class: "Ams.Core.Prosody.PauseTransform"
interfaces:
  - "System.IEquatable<Ams.Core.Prosody.PauseAdjust>"
member_count: 1
dependency_count: 1
pattern: ~
tags:
  - class
---

# PauseAdjust

> Record in `Ams.Core.Prosody`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseModels.cs`

**Inherits from**: [[PauseTransform]]

**Implements**:
- IEquatable

## Dependencies
- [[Ams.Core.Prosody.PauseClass]] (`Class`)

## Properties
- `LeftSentenceId`: int
- `RightSentenceId`: int
- `Class`: PauseClass
- `OriginalDurationSec`: double
- `TargetDurationSec`: double
- `StartSec`: double
- `EndSec`: double
- `HasGapHint`: bool
- `IsIntraSentence`: bool

## Members


---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/AlignmentOptions.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Services.Alignment.AnchorComputationOptions>"
member_count: 0
dependency_count: 0
pattern: ~
tags:
  - class
---

# AnchorComputationOptions

> Record in `Ams.Core.Services.Alignment`

**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/AlignmentOptions.cs`

**Implements**:
- IEquatable

## Properties
- `NGram`: int
- `TargetPerTokens`: int
- `MinSeparation`: int
- `AllowBoundaryCross`: bool
- `UseDomainStopwords`: bool
- `DetectSection`: bool
- `AsrPrefixTokens`: int
- `EmitWindows`: bool
- `SectionOverride`: SectionRange?
- `TryResolveSectionFromLabels`: bool


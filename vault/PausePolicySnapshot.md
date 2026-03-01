---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseAdjustmentsDocument.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Prosody.PausePolicySnapshot>"
member_count: 3
dependency_count: 1
pattern: ~
tags:
  - class
---

# PausePolicySnapshot

> Record in `Ams.Core.Prosody`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseAdjustmentsDocument.cs`

**Implements**:
- IEquatable

## Dependencies
- [[PauseWindowSnapshot]] (`Comma`)

## Properties
- `Comma`: PauseWindowSnapshot
- `Sentence`: PauseWindowSnapshot
- `Paragraph`: PauseWindowSnapshot
- `HeadOfChapter`: double
- `PostChapterRead`: double
- `Tail`: double
- `KneeWidth`: double
- `RatioInside`: double
- `RatioOutside`: double
- `PreserveTopQuantile`: double

## Members
- [[PausePolicySnapshot.FromPolicy]]
- [[PausePolicySnapshot.ToPolicy]]


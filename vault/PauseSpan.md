---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseModels.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Prosody.PauseSpan>"
member_count: 1
dependency_count: 2
pattern: ~
tags:
  - class
---

# PauseSpan

> Record in `Ams.Core.Prosody`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseModels.cs`

**Implements**:
- IEquatable

## Dependencies
- [[Ams.Core.Prosody.PauseClass]] (`Class`)
- [[Ams.Core.Prosody.PauseProvenance]] (`Provenance`)

## Properties
- `LeftSentenceId`: int
- `RightSentenceId`: int
- `StartSec`: double
- `EndSec`: double
- `DurationSec`: double
- `Class`: PauseClass
- `HasGapHint`: bool
- `CrossesParagraph`: bool
- `CrossesChapterHead`: bool
- `Provenance`: PauseProvenance

## Members


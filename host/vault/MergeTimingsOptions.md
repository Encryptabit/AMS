---
namespace: "Ams.Core.Application.Commands"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Commands/MergeTimingsCommand.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Application.Commands.MergeTimingsOptions>"
member_count: 0
dependency_count: 0
pattern: ~
tags:
  - class
---

# MergeTimingsOptions

> Record in `Ams.Core.Application.Commands`

**Path**: `Projects/AMS/host/Ams.Core/Application/Commands/MergeTimingsCommand.cs`

**Implements**:
- IEquatable

## Properties
- `HydrateFile`: FileInfo?
- `TranscriptFile`: FileInfo?
- `TextGridFile`: FileInfo?
- `AlignmentRootDirectory`: DirectoryInfo?
- `ApplyToHydrate`: bool
- `ApplyToTranscript`: bool


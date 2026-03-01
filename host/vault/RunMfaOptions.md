---
namespace: "Ams.Core.Application.Commands"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Commands/RunMfaCommand.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Application.Commands.RunMfaOptions>"
member_count: 0
dependency_count: 0
pattern: ~
tags:
  - class
---

# RunMfaOptions

> Record in `Ams.Core.Application.Commands`

**Path**: `Projects/AMS/host/Ams.Core/Application/Commands/RunMfaCommand.cs`

**Implements**:
- IEquatable

## Properties
- `AudioFile`: FileInfo?
- `HydrateFile`: FileInfo?
- `TextGridFile`: FileInfo?
- `AlignmentRootDirectory`: DirectoryInfo?
- `ChapterDirectory`: DirectoryInfo?
- `UseDedicatedProcess`: bool
- `WorkspaceRoot`: string?


---
namespace: "Ams.Core.Runtime.Workspace"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Workspace/IWorkspace.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Runtime.Workspace.ChapterOpenOptions>"
member_count: 0
dependency_count: 0
pattern: ~
tags:
  - class
---

# ChapterOpenOptions

> Record in `Ams.Core.Runtime.Workspace`

**Path**: `Projects/AMS/host/Ams.Core/Runtime/Workspace/IWorkspace.cs`

**Implements**:
- IEquatable

## Properties
- `BookIndexFile`: FileInfo?
- `AsrFile`: FileInfo?
- `TranscriptFile`: FileInfo?
- `HydrateFile`: FileInfo?
- `AudioFile`: FileInfo?
- `ChapterDirectory`: DirectoryInfo?
- `ChapterId`: string?
- `ReloadBookIndex`: bool


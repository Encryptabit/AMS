---
namespace: "Ams.Core.Runtime.Interfaces"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Interfaces/IChapterManager.cs"
access_modifier: "public"
base_class: ~
interfaces: []
member_count: 10
dependency_count: 0
pattern: ~
tags:
  - interface
---

# IChapterManager

> Interface in `Ams.Core.Runtime.Interfaces`

**Path**: `Projects/AMS/host/Ams.Core/Runtime/Interfaces/IChapterManager.cs`

## Properties
- `Count`: int
- `Descriptors`: IReadOnlyList<ChapterDescriptor>
- `Current`: ChapterContext

## Members
- [[IChapterManager.Load]]
- [[IChapterManager.Load_2]]
- [[IChapterManager.Contains]]
- [[IChapterManager.CreateContext]]
- [[IChapterManager.UpsertDescriptor]]
- [[IChapterManager.TryMoveNext]]
- [[IChapterManager.TryMovePrevious]]
- [[IChapterManager.Reset]]
- [[IChapterManager.Deallocate]]
- [[IChapterManager.DeallocateAll]]

## Known Implementors
- [[ChapterManager]]


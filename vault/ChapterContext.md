---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterContext.cs"
access_modifier: "public"
base_class: ~
interfaces: []
member_count: 5
dependency_count: 2
pattern: ~
tags:
  - class
---

# ChapterContext

> Class in `Ams.Core.Runtime.Chapter`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterContext.cs`

## Dependencies
- [[BookContext]] (`book`)
- [[ChapterDescriptor]] (`descriptor`)

## Properties
- `Book`: BookContext
- `Descriptor`: ChapterDescriptor
- `Documents`: ChapterDocuments
- `Audio`: AudioBufferManager
- `_resolver`: IArtifactResolver
- `_resolvedSection`: SectionRange?

## Members
- [[ChapterContext..ctor]]
- [[ChapterContext.Save]]
- [[ChapterContext.ResolveArtifactFile]]
- [[ChapterContext.GetOrResolveSection]]
- [[ChapterContext.SetDetectedSection]]


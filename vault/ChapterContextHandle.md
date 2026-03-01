---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterContextHandle.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IDisposable"
member_count: 5
dependency_count: 2
pattern: ~
tags:
  - class
---

# ChapterContextHandle

> Class in `Ams.Core.Runtime.Chapter`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterContextHandle.cs`

**Implements**:
- IDisposable

## Dependencies
- [[BookContext]] (`bookContext`)
- [[ChapterContext]] (`chapterContext`)

## Properties
- `Book`: BookContext
- `Chapter`: ChapterContext
- `Managers`: Dictionary<ManagerKey, BookManager>
- `Sync`: object
- `_bookContext`: BookContext
- `_chapterContext`: ChapterContext
- `_disposed`: bool

## Members
- [[ChapterContextHandle..ctor]]
- [[ChapterContextHandle.Create]]
- [[ChapterContextHandle.Save]]
- [[ChapterContextHandle.Dispose]]
- [[ChapterContextHandle.GetOrCreateManager]]


---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookManager.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Runtime.Book.ChapterDescriptor>"
member_count: 1
dependency_count: 0
pattern: ~
tags:
  - class
---

# ChapterDescriptor

> Record in `Ams.Core.Runtime.Book`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookManager.cs`

**Implements**:
- IEquatable

## Properties
- `ChapterId`: string
- `RootPath`: string
- `AudioBuffers`: IReadOnlyList<AudioBufferDescriptor>
- `Documents`: IReadOnlyDictionary<string, string>?
- `Aliases`: IReadOnlyCollection<string>
- `BookStartWord`: int?
- `BookEndWord`: int?

## Members
- [[ChapterDescriptor..ctor]]


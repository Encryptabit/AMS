---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookManager.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Runtime.Book.BookDescriptor>"
member_count: 1
dependency_count: 0
pattern: ~
tags:
  - class
---

# BookDescriptor

> Record in `Ams.Core.Runtime.Book`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookManager.cs`

**Implements**:
- IEquatable

## Properties
- `BookId`: string
- `RootPath`: string
- `Chapters`: IReadOnlyList<ChapterDescriptor>
- `Documents`: IReadOnlyDictionary<string, string>?

## Members
- [[BookDescriptor..ctor]]


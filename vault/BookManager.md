---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookManager.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "Ams.Core.Runtime.Interfaces.IBookManager"
member_count: 9
dependency_count: 1
pattern: ~
tags:
  - class
---

# BookManager

> Class in `Ams.Core.Runtime.Book`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookManager.cs`

**Implements**:
- [[IBookManager]]

## Dependencies
- [[Ams.Core.Runtime.Artifacts.IArtifactResolver_]] (`resolver`)

## Properties
- `Count`: int
- `Current`: BookContext
- `_descriptors`: IReadOnlyList<BookDescriptor>
- `_cache`: Dictionary<string, BookContext>
- `_artifactResolver`: IArtifactResolver
- `_cursor`: int

## Members
- [[BookManager..ctor]]
- [[BookManager.Load]]
- [[BookManager.Load_2]]
- [[BookManager.TryMoveNext]]
- [[BookManager.TryMovePrevious]]
- [[BookManager.Reset]]
- [[BookManager.Deallocate]]
- [[BookManager.DeallocateAll]]
- [[BookManager.GetOrCreate]]


---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookContext.cs"
access_modifier: "public"
base_class: ~
interfaces: []
member_count: 3
dependency_count: 2
pattern: ~
tags:
  - class
---

# BookContext

> Class in `Ams.Core.Runtime.Book`

**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookContext.cs`

## Dependencies
- [[BookDescriptor]] (`descriptor`)
- [[IArtifactResolver]] (`resolver`)

## Properties
- `Resolver`: IArtifactResolver
- `Descriptor`: BookDescriptor
- `Documents`: BookDocuments
- `Chapters`: ChapterManager
- `Audio`: BookAudio
- `_resolver`: IArtifactResolver

## Members
- [[BookContext..ctor]]
- [[BookContext.Save]]
- [[BookContext.ResolveArtifactFile]]


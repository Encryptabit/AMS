---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookModels.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Runtime.Book.BookIndex>"
member_count: 1
dependency_count: 1
pattern: ~
tags:
  - class
---

# BookIndex

> Record in `Ams.Core.Runtime.Book`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookModels.cs`

**Implements**:
- IEquatable

## Dependencies
- [[BookTotals]] (`Totals`)

## Properties
- `SourceFile`: string
- `SourceFileHash`: string
- `IndexedAt`: DateTime
- `Title`: string?
- `Author`: string?
- `Totals`: BookTotals
- `Words`: BookWord[]
- `Sentences`: SentenceRange[]
- `Paragraphs`: ParagraphRange[]
- `Sections`: SectionRange[]
- `BuildWarnings`: string[]?

## Members


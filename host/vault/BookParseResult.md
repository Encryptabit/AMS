---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/IBookServices.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Runtime.Book.BookParseResult>"
member_count: 1
dependency_count: 0
pattern: ~
tags:
  - class
---

# BookParseResult

> Record in `Ams.Core.Runtime.Book`

**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/IBookServices.cs`

**Implements**:
- IEquatable

## Properties
- `Text`: string
- `Title`: string?
- `Author`: string?
- `Metadata`: IReadOnlyDictionary<string, object>?
- `Paragraphs`: IReadOnlyList<ParsedParagraph>?

## Members


---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# BookIndexer::ShouldSkipParagraphFromIndex
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

## Summary
**Determines whether a paragraph should be excluded from indexing based on style and TOC-like formatting signals.**

`ShouldSkipParagraphFromIndex` applies exclusion heuristics to decide whether a paragraph should be omitted from lexical indexing. It first short-circuits `true` when a non-empty `style` matches `IsNonSectionParagraphStyle(style)`. It then trims `text`; blank/empty text returns `false` (not explicitly skipped here), while non-empty text is skipped only if `LooksLikeTableOfContentsEntry(trimmed)` returns true. The method ignores `kind` in current implementation despite accepting it as a parameter.


#### [[BookIndexer.ShouldSkipParagraphFromIndex]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool ShouldSkipParagraphFromIndex(string text, string style, string kind)
```

**Calls ->**
- [[BookIndexer.IsNonSectionParagraphStyle]]
- [[BookIndexer.LooksLikeTableOfContentsEntry]]

**Called-by <-**
- [[BookIndexer.Process]]


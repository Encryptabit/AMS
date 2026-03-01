---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 10
fan_in: 3
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# BookIndexer::IsNonSectionParagraphStyle
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

## Summary
**Flags paragraph styles that represent non-section content and should not trigger section logic.**

`IsNonSectionParagraphStyle` identifies styles that should be excluded from section detection using case-insensitive keyword checks. It returns `false` for null/whitespace input, then matches style text against a denylist including TOC/table-of-contents variants, captions, headers/footers, page-number forms, and index markers. The method is a pure heuristic filter used to suppress structural false positives.


#### [[BookIndexer.IsNonSectionParagraphStyle]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool IsNonSectionParagraphStyle(string style)
```

**Called-by <-**
- [[BookIndexer.LooksLikeHeadingStyle]]
- [[BookIndexer.ShouldSkipParagraphFromIndex]]
- [[BookIndexer.ShouldStartSection]]


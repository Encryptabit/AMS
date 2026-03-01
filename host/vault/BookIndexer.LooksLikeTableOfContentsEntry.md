---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 9
fan_in: 3
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# BookIndexer::LooksLikeTableOfContentsEntry
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

## Summary
**Identifies whether a line appears to be a table-of-contents entry with trailing page-number formatting.**

`LooksLikeTableOfContentsEntry` detects TOC-style lines using formatting cues rather than semantic parsing. It returns `false` for blank input, then matches common patterns: dotted leaders (`"...."` or regex `\.{2,}\s*\d+$`), multi-space + trailing page number (`[ \t]{2,}\d+$`), and tab-delimited lines whose last field is all digits. If none of these patterns match, it returns `false`. This heuristic is used to suppress false heading/section detections from contents pages.


#### [[BookIndexer.LooksLikeTableOfContentsEntry]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool LooksLikeTableOfContentsEntry(string text)
```

**Called-by <-**
- [[BookIndexer.LooksLikeStandaloneTitle]]
- [[BookIndexer.ShouldSkipParagraphFromIndex]]
- [[BookIndexer.ShouldStartSection]]


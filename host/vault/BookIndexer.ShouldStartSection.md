---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 7
fan_in: 2
fan_out: 5
tags:
  - method
  - llm/utility
  - llm/validation
---
# BookIndexer::ShouldStartSection
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

## Summary
**Determines whether a paragraph should be treated as a section boundary based on text and style heuristics.**

`ShouldStartSection` applies a layered heuristic to decide whether a paragraph should open a new section. It rejects empty text and non-section styles first, then computes signals from content/style (`LooksLikeSectionHeading`, `LooksLikeHeadingStyle`, `LooksLikeStandaloneTitle`). If heading keywords are detected, it suppresses likely TOC entries via `LooksLikeTableOfContentsEntry`; otherwise it only accepts style-suggested headings that also look like standalone titles. The method returns `false` by default when no rule path matches.


#### [[BookIndexer.ShouldStartSection]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool ShouldStartSection(string text, string style, string kind)
```

**Calls ->**
- [[BookIndexer.IsNonSectionParagraphStyle]]
- [[BookIndexer.LooksLikeHeadingStyle]]
- [[BookIndexer.LooksLikeSectionHeading]]
- [[BookIndexer.LooksLikeStandaloneTitle]]
- [[BookIndexer.LooksLikeTableOfContentsEntry]]

**Called-by <-**
- [[BookIndexer.FoldAdjacentHeadings]]
- [[BookIndexer.Process]]


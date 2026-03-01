---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 7
fan_in: 2
fan_out: 5
tags:
  - method
---
# BookIndexer::ShouldStartSection
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`


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


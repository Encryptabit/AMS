---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 10
fan_in: 3
fan_out: 0
tags:
  - method
---
# BookIndexer::IsNonSectionParagraphStyle
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`


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


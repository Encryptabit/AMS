---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 9
fan_in: 3
fan_out: 0
tags:
  - method
---
# BookIndexer::LooksLikeTableOfContentsEntry
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`


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


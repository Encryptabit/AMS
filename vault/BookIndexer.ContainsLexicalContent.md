---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 4
fan_in: 3
fan_out: 0
tags:
  - method
---
# BookIndexer::ContainsLexicalContent
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`


#### [[BookIndexer.ContainsLexicalContent]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool ContainsLexicalContent(string text)
```

**Called-by <-**
- [[BookIndexer.CollectLexicalTokens]]
- [[BookIndexer.FoldAdjacentHeadings]]
- [[BookIndexer.Process]]


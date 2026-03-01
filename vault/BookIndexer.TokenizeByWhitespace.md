---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 8
fan_in: 2
fan_out: 0
tags:
  - method
---
# BookIndexer::TokenizeByWhitespace
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`


#### [[BookIndexer.TokenizeByWhitespace]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IEnumerable<string> TokenizeByWhitespace(string text)
```

**Called-by <-**
- [[BookIndexer.CollectLexicalTokens]]
- [[BookIndexer.Process]]


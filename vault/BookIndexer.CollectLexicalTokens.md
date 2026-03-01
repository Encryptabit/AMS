---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 4
tags:
  - method
---
# BookIndexer::CollectLexicalTokens
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`


#### [[BookIndexer.CollectLexicalTokens]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IEnumerable<string> CollectLexicalTokens(List<(string Text, string Style, string Kind)> paragraphs)
```

**Calls ->**
- [[PronunciationHelper.NormalizeForLookup]]
- [[BookIndexer.ContainsLexicalContent]]
- [[BookIndexer.NormalizeTokenSurface]]
- [[BookIndexer.TokenizeByWhitespace]]

**Called-by <-**
- [[BookIndexer.CreateIndexAsync]]


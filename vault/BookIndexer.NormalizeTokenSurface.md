---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 2
tags:
  - method
---
# BookIndexer::NormalizeTokenSurface
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`


#### [[BookIndexer.NormalizeTokenSurface]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string NormalizeTokenSurface(string token)
```

**Calls ->**
- [[TextNormalizer.NormalizeTypography]]
- [[BookIndexer.TrimOuterQuotes]]

**Called-by <-**
- [[BookIndexer.CollectLexicalTokens]]
- [[BookIndexer.Process]]


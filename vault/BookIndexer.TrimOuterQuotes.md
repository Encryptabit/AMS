---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 1
tags:
  - method
---
# BookIndexer::TrimOuterQuotes
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`


#### [[BookIndexer.TrimOuterQuotes]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string TrimOuterQuotes(string value)
```

**Calls ->**
- [[BookIndexer.IsQuoteChar]]

**Called-by <-**
- [[BookIndexer.NormalizeTokenSurface]]


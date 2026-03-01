---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 15
fan_in: 1
fan_out: 1
tags:
  - method
  - danger/high-complexity
---
# BookIndexer::LooksLikeStandaloneTitle
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

> [!danger] High Complexity (15)
> Cyclomatic complexity: 15. Consider refactoring into smaller methods.


#### [[BookIndexer.LooksLikeStandaloneTitle]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool LooksLikeStandaloneTitle(string text)
```

**Calls ->**
- [[BookIndexer.LooksLikeTableOfContentsEntry]]

**Called-by <-**
- [[BookIndexer.ShouldStartSection]]


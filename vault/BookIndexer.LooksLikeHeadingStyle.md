---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 16
fan_in: 2
fan_out: 1
tags:
  - method
  - danger/high-complexity
---
# BookIndexer::LooksLikeHeadingStyle
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

> [!danger] High Complexity (16)
> Cyclomatic complexity: 16. Consider refactoring into smaller methods.


#### [[BookIndexer.LooksLikeHeadingStyle]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool LooksLikeHeadingStyle(string style, string kind)
```

**Calls ->**
- [[BookIndexer.IsNonSectionParagraphStyle]]

**Called-by <-**
- [[BookIndexer.Process]]
- [[BookIndexer.ShouldStartSection]]


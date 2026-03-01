---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDiscoveryService.cs"
access_modifier: "public"
complexity: 7
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# ChapterFileComparer::Compare
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDiscoveryService.cs`

## Summary
**Compares two chapter files using numeric-first, stem-aware sorting semantics.**

`Compare` implements numeric-aware ordering for chapter files with deterministic null and tie-break behavior. It first handles reference/null comparisons, then computes sort keys via `GetSortKey` (category + first numeric token + lowercase stem). Comparison precedence is: `Category` (numbered stems before non-numbered), `PrimaryNumber`, normalized lowercase name (`Ordinal`), and finally original filename (`OrdinalIgnoreCase`) as a stable tie-breaker. This yields predictable chapter ordering even with mixed filename patterns.


#### [[Ams.Core.Runtime.Chapter.ChapterDiscoveryService.ChapterFileComparer.Compare_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public int Compare(FileInfo x, FileInfo y)
```

**Calls ->**
- [[Ams.Core.Runtime.Chapter.ChapterDiscoveryService.ChapterFileComparer.GetSortKey_2]]


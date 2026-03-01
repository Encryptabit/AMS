---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDiscoveryService.cs"
access_modifier: "internal"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# ChapterFileComparer::CompareStemStrings
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDiscoveryService.cs`

## Summary
**Compares chapter stem strings using numeric-first ordering with deterministic lexical tie-breaking.**

`CompareStemStrings` applies the same numeric-aware sort strategy as `Compare`, but directly on stem strings. It handles reference/null cases first, then computes normalized `SortKey`s via `GetStemSortKey` and compares by `Category` (numeric-stem vs non-numeric), `PrimaryNumber`, then lowercase name (`Ordinal`). This provides deterministic ordering for unmatched chapter titles when only stem text is available.


#### [[Ams.Core.Runtime.Chapter.ChapterDiscoveryService.ChapterFileComparer.CompareStemStrings]]
##### What it does:
<member name="M:Ams.Core.Runtime.Chapter.ChapterDiscoveryService.ChapterFileComparer.CompareStemStrings(System.String,System.String)">
    <summary>
    Compares two stem strings using numeric-aware logic.
    Used when FileInfo is not available.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static int CompareStemStrings(string x, string y)
```

**Calls ->**
- [[Ams.Core.Runtime.Chapter.ChapterDiscoveryService.ChapterFileComparer.GetStemSortKey]]

**Called-by <-**
- [[ChapterDiscoveryService.DiscoverChaptersCore]]


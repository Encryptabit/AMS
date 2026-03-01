---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# BookIndexer::CombineHeadingTitles
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

## Summary
**Combines two heading fragments into a single title while avoiding duplicate or overlapping text.**

`CombineHeadingTitles` merges two heading strings using normalization and deduplication heuristics. It first handles null/whitespace fallbacks (`first` or `second`), then trims both inputs and returns the second when titles are equal case-insensitively. It also collapses containment cases (`b.StartsWith(a)` returns `b`, `a.EndsWith(b)` returns `a`) to avoid redundant concatenation. Only when neither rule matches does it join them as `"{a} — {b}"`.


#### [[BookIndexer.CombineHeadingTitles]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string CombineHeadingTitles(string first, string second)
```

**Called-by <-**
- [[BookIndexer.FoldAdjacentHeadings]]


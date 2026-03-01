---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs"
access_modifier: "public"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
---
# AnchorDiscovery::IndexNGrams
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs`

## Summary
**Creates a positional lookup of n-gram strings to every token index where each n-gram begins.**

IndexNGrams builds an ordinal-keyed inverted index from each contiguous n-gram in `toks` to all start offsets where it occurs. It returns early with an empty dictionary when `toks.Count < n`; otherwise it scans `i = 0..toks.Count-n`, materializes each key via `string.Join("|", toks.Skip(i).Take(n))`, lazily allocates the postings list, and appends `i`. The result maps each normalized n-gram string to its positional occurrences for downstream matching.


#### [[AnchorDiscovery.IndexNGrams]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Dictionary<string, List<int>> IndexNGrams(IReadOnlyList<string> toks, int n)
```

**Called-by <-**
- [[AnchorDiscovery.Collect]]


---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
---
# AnchorDiscovery::Collect
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs`

## Summary
**Builds candidate book-to-ASR anchors from shared n-grams that pass occurrence rules and content/boundary validation.**

Collect builds n-gram occurrence maps for both token streams using `IndexNGrams`, then scans each book n-gram key and keeps only entries that satisfy the caller-supplied `okBook`/`okAsr` occurrence predicates. For accepted keys, it uses the first positions (`bp`, `ap`) and applies semantic filters: `PassContent` for stopword/content quality and, when enabled, `policy.DisallowBoundaryCross` with `CrossesSentence(bookSentIdx, bp, n)` to reject cross-sentence spans. It returns raw `Anchor(bp, ap)` candidates, leaving ordering/monotonic pruning to the caller.


#### [[AnchorDiscovery.Collect]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<Anchor> Collect(IReadOnlyList<string> book, IReadOnlyList<int> bookSentIdx, IReadOnlyList<string> asr, int n, ISet<string> stop, Func<List<int>, bool> okBook, Func<List<int>, bool> okAsr, AnchorPolicy policy)
```

**Calls ->**
- [[AnchorDiscovery.CrossesSentence]]
- [[AnchorDiscovery.IndexNGrams]]
- [[AnchorDiscovery.PassContent]]

**Called-by <-**
- [[AnchorDiscovery.SelectAnchors]]


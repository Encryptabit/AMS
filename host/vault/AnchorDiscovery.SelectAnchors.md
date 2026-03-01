---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs"
access_modifier: "public"
complexity: 6
fan_in: 3
fan_out: 4
tags:
  - method
  - llm/utility
  - llm/validation
---
# AnchorDiscovery::SelectAnchors
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs`

## Summary
**Discovers robust book-to-ASR anchor points by iteratively relaxing n-gram matching constraints and then filtering to a monotonic subsequence.**

SelectAnchors performs n-gram anchor discovery between `bookTokens` and `asrTokens` using `Collect` with strict uniqueness predicates (`list.Count == 1` on both sides), defaulting stopwords to an ordinal `HashSet` when none are provided. It then applies density control (`desired = Max(1, bookTokens.Count / Max(1, TargetPerTokens))`): first retries with relaxed duplicate tolerance via `FarApart(..., MinSeparation)`, and if still sparse and `NGram > 2`, recursively re-runs with `NGram - 1`. Finally it sorts anchors by book position and enforces monotonic alignment with `LisByAp` over `(Bp, Ap)` pairs, returning only the LIS-consistent anchors.


#### [[AnchorDiscovery.SelectAnchors]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IReadOnlyList<Anchor> SelectAnchors(IReadOnlyList<string> bookTokens, IReadOnlyList<int> bookSentenceIndex, IReadOnlyList<string> asrTokens, AnchorPolicy policy)
```

**Calls ->**
- [[AnchorDiscovery.Collect]]
- [[FarApart]]
- [[AnchorDiscovery.LisByAp]]
- [[AnchorDiscovery.SelectAnchors]]

**Called-by <-**
- [[AnchorDiscovery.SelectAnchors_2]]
- [[AnchorDiscovery.SelectAnchors]]
- [[AnchorDiscoveryTests.UniqueTrigrams_ProduceAnchors]]


---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs"
access_modifier: "public"
complexity: 8
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AnchorDiscovery::LisByAp
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs`

## Summary
**Returns a monotonic anchor subsequence by extracting the LIS of alignment positions on the ASR axis.**

LisByAp computes a longest increasing subsequence over `ap` values (after caller-side ordering by `bp`) using the classic O(n log n) patience-sorting approach with binary search. It maintains `tails` (best ending index per LIS length) and `prev` predecessor links, updating insertion position `lo` by comparing `pairs[tails[mid]].ap < pairs[i].ap`. After the forward pass, it reconstructs the subsequence by backtracking from `tails[size-1]` through `prev`, then reverses to return increasing order as `List<(int bp, int ap)>`. It returns an empty list when input is empty.


#### [[AnchorDiscovery.LisByAp]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static List<(int bp, int ap)> LisByAp(IReadOnlyList<(int bp, int ap)> pairs)
```

**Called-by <-**
- [[AnchorDiscovery.SelectAnchors]]
- [[AnchorDiscoveryTests.LisEnforcesMonotonicity]]


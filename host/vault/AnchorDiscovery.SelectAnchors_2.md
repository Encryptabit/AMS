---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs"
access_modifier: "public"
complexity: 5
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AnchorDiscovery::SelectAnchors
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs`

## Summary
**Select anchors within a specified book token range by slicing the inputs, delegating to the main anchor-selection routine, and translating anchor positions back to original indices.**

This overload constrains anchor discovery to an inclusive book-token window and then reuses the core selector on that slice. It first validates `bookTokens.Count == bookSentenceIndex.Count` and checks `bookStart/bookEnd` bounds, throwing `ArgumentException` or `ArgumentOutOfRangeException` on invalid input. It computes `len = bookEnd - bookStart + 1`, creates `subBook`/`subSent` via `Skip(...).Take(len).ToList()`, calls `SelectAnchors(subBook, subSent, asrTokens, policy)`, and remaps each result by adding `bookStart` back to `Anchor.Bp` while leaving `Anchor.Ap` unchanged.


#### [[AnchorDiscovery.SelectAnchors_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IReadOnlyList<Anchor> SelectAnchors(IReadOnlyList<string> bookTokens, IReadOnlyList<int> bookSentenceIndex, IReadOnlyList<string> asrTokens, AnchorPolicy policy, int bookStart, int bookEnd)
```

**Calls ->**
- [[AnchorDiscovery.SelectAnchors]]

**Called-by <-**
- [[AnchorPipeline.ComputeAnchors]]
- [[SectionLocatorTests.AnchorSelection_Respects_Book_Window]]


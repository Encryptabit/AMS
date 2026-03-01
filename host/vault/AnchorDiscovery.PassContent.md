---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AnchorDiscovery::PassContent
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs`

## Summary
**Determines whether an n-gram is content-rich enough to be used as an anchor candidate.**

PassContent validates an n-gram span starting at `i` against stopword-based content heuristics. It counts non-stopword tokens across `toks[i..i+n-1]`, rejects low-information trigrams+ when `n >= 3 && content < 2`, and then enforces edge quality by rejecting spans whose first or last token is a stopword. It returns `true` only when the span has sufficient internal content and non-stopword boundaries.


#### [[AnchorDiscovery.PassContent]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool PassContent(IReadOnlyList<string> toks, int i, int n, ISet<string> stop)
```

**Called-by <-**
- [[AnchorDiscovery.Collect]]


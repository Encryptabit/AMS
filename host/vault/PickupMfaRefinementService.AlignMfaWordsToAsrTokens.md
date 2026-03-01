---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/PickupMfaRefinementService.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 2
tags:
  - method
---
# PickupMfaRefinementService::AlignMfaWordsToAsrTokens
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/PickupMfaRefinementService.cs`


#### [[PickupMfaRefinementService.AlignMfaWordsToAsrTokens]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Dictionary<int, (double Start, double End)> AlignMfaWordsToAsrTokens(IReadOnlyList<TextGridInterval> intervals, IReadOnlyList<string> alignmentWords, IReadOnlyList<int> wordToTokenIndex)
```

**Calls ->**
- [[Log.Debug]]
- [[MfaTimingMerger.MergeAndApply]]

**Called-by <-**
- [[PickupMfaRefinementService.RefineAsrTimingsAsync]]


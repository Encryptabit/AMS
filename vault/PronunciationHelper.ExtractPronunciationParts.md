---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/PronunciationHelper.cs"
access_modifier: "public"
complexity: 13
fan_in: 5
fan_out: 3
tags:
  - method
---
# PronunciationHelper::ExtractPronunciationParts
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/PronunciationHelper.cs`


#### [[PronunciationHelper.ExtractPronunciationParts]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IReadOnlyList<string> ExtractPronunciationParts(string token)
```

**Calls ->**
- [[TextNormalizer.NormalizeTypography]]
- [[PronunciationHelper.ConvertNumberToWordsSafe]]
- [[Flush]]

**Called-by <-**
- [[MfaWorkflow.PrepareLabLine]]
- [[MfaTimingMerger.TokenizeForAlignment]]
- [[PronunciationHelper.NormalizeForLookup]]
- [[BookModelsTests.PronunciationHelper_NormalizesNumbersToWords]]
- [[PickupMfaRefinementService.BuildAlignmentWords]]


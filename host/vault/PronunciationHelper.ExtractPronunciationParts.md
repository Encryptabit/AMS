---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/PronunciationHelper.cs"
access_modifier: "public"
complexity: 13
fan_in: 5
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/validation
---
# PronunciationHelper::ExtractPronunciationParts
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/PronunciationHelper.cs`

## Summary
**Breaks a token into normalized pronunciation lookup parts, including number-to-words expansion and punctuation-aware segmentation.**

`ExtractPronunciationParts` tokenizes a string into pronunciation-ready lexical segments with typography normalization and mixed alphanumeric handling. It normalizes typography first, scans characters into a mutable buffer for letter runs (lowercased), flushes segments on separators, preserves apostrophes, and splits on hyphens/punctuation boundaries. Digit runs (including `,`/`_` grouping characters) are converted via `ConvertNumberToWordsSafe`; resulting words are appended as separate segments, or the raw digits fallback is used when conversion fails. The method returns a filtered array of non-empty parts.


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


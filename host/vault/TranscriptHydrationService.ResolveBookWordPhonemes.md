---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# TranscriptHydrationService::ResolveBookWordPhonemes
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs`

## Summary
**Returns the best available phoneme variants for a book word from embedded data or fallback pronunciation lookups.**

`ResolveBookWordPhonemes` resolves phoneme variants for a book word using a two-tier fallback. It returns intrinsic `word.Phonemes` when present, otherwise normalizes `word.Text` via `PronunciationHelper.NormalizeForLookup` and attempts dictionary lookup in `fallbackPronunciations`. If normalization fails or lookup has no non-empty mapping, it returns `null` (implementation signature is nullable `string[]?`).


#### [[TranscriptHydrationService.ResolveBookWordPhonemes]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string[] ResolveBookWordPhonemes(BookWord word, IReadOnlyDictionary<string, string[]> fallbackPronunciations)
```

**Calls ->**
- [[PronunciationHelper.NormalizeForLookup]]

**Called-by <-**
- [[TranscriptHydrationService.BuildBookScoringView]]


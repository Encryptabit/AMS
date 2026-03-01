---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/PronunciationHelper.cs"
access_modifier: "public"
complexity: 3
fan_in: 11
fan_out: 1
tags:
  - method
  - danger/high-fan-in
---
# PronunciationHelper::NormalizeForLookup
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/PronunciationHelper.cs`

> [!danger] High Fan-In (11)
> This method is called by 11 other methods. Changes here have wide impact.


#### [[PronunciationHelper.NormalizeForLookup]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static string NormalizeForLookup(string token)
```

**Calls ->**
- [[PronunciationHelper.ExtractPronunciationParts]]

**Called-by <-**
- [[BuildIndexCommand.CountMissingPhonemes]]
- [[MfaPronunciationProvider.NormalizeVariantKey]]
- [[BookPhonemePopulator.PopulateMissingAsync]]
- [[BookIndexer.CollectLexicalTokens]]
- [[BookIndexer.Process]]
- [[TranscriptHydrationService.BuildAsrScoringView]]
- [[TranscriptHydrationService.BuildPronunciationFallbackAsync]]
- [[TranscriptHydrationService.ResolveBookWordPhonemes]]
- [[TranscriptIndexService.BuildAsrPhonemeViewAsync]]
- [[PipelineService.CountMissingPhonemes]]
- [[BookModelsTests.PronunciationHelper_NormalizesNumbersToWords]]


---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/PronunciationHelper.cs"
access_modifier: "public"
complexity: 3
fan_in: 11
fan_out: 1
tags:
  - method
  - danger/high-fan-in
  - llm/utility
  - llm/validation
---
# PronunciationHelper::NormalizeForLookup
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/PronunciationHelper.cs`

> [!danger] High Fan-In (11)
> This method is called by 11 other methods. Changes here have wide impact.

## Summary
**Normalizes token text into a canonical, space-delimited lexeme key for pronunciation lookup.**

`NormalizeForLookup` converts a raw token into a pronunciation-lookup lexeme by delegating token decomposition to `ExtractPronunciationParts(token)`. It returns `null` for null/whitespace input or when no normalized parts are produced, otherwise joins parts with single spaces (`string.Join(" ", parts)`). Because `ExtractPronunciationParts` lowercases letters, handles punctuation boundaries, and expands numeric sequences, this method provides a canonical lookup key for pronunciation dictionaries.


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


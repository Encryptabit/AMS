---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Common/TextNormalizer.cs"
access_modifier: "public"
complexity: 4
fan_in: 12
fan_out: 2
tags:
  - method
  - danger/high-fan-in
  - llm/utility
  - llm/validation
---
# TextNormalizer::Normalize
**Path**: `Projects/AMS/host/Ams.Core/Common/TextNormalizer.cs`

> [!danger] High Fan-In (12)
> This method is called by 12 other methods. Changes here have wide impact.

## Summary
**Canonicalizes free-form text into a consistent normalized form for downstream comparison, scoring, and token extraction.**

`Normalize` returns `string.Empty` for null/whitespace input, then trims, runs `NormalizeTypography`, and lowercases with `ToLowerInvariant()`. If `expandContractions` is enabled, it expands entries from `CommonContractions` using case-insensitive whole-word regex replacement. It removes punctuation via `PunctuationRegex` (preserving apostrophes), then either removes numeric tokens or converts parsed integers in the 0-999 range using `NumberToWords` while leaving other numbers unchanged. It finishes by collapsing whitespace with `WhitespaceRegex` and trimming.


#### [[TextNormalizer.Normalize]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static string Normalize(string text, bool expandContractions = true, bool removeNumbers = false)
```

**Calls ->**
- [[TextNormalizer.NormalizeTypography]]
- [[TextNormalizer.NumberToWords]]

**Called-by <-**
- [[TextCommand.Create]]
- [[TextNormalizer.CalculateSimilarity]]
- [[SectionLocator.DetectSection]]
- [[TextDiffAnalyzer.NormalizeForDisplay]]
- [[TextDiffAnalyzer.NormalizeForScoring]]
- [[TranscriptHydrationService.BuildAsrScoringView]]
- [[TranscriptHydrationService.BuildBookScoringView]]
- [[ScriptValidator.CalculateCharacterErrorRate]]
- [[ScriptValidator.ExtractWordsFromAsrResponse]]
- [[ScriptValidator.ExtractWordsFromScript]]
- [[ScriptValidator.GenerateSegmentStats]]
- [[TextNormalizerTests.Normalize_ShouldNormalizeTextCorrectly]]


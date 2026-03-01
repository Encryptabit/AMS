---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Common/TextNormalizer.cs"
access_modifier: "public"
complexity: 4
fan_in: 12
fan_out: 2
tags:
  - method
  - danger/high-fan-in
---
# TextNormalizer::Normalize
**Path**: `home/cari/repos/AMS/host/Ams.Core/Common/TextNormalizer.cs`

> [!danger] High Fan-In (12)
> This method is called by 12 other methods. Changes here have wide impact.


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


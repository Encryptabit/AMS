---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Common/TextNormalizer.cs"
access_modifier: "public"
complexity: 2
fan_in: 7
fan_out: 0
tags:
  - method
---
# TextNormalizer::TokenizeWords
**Path**: `home/cari/repos/AMS/host/Ams.Core/Common/TextNormalizer.cs`


#### [[TextNormalizer.TokenizeWords]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static string[] TokenizeWords(string text)
```

**Called-by <-**
- [[SectionLocator.DetectSection]]
- [[TextDiffAnalyzer.Tokenize]]
- [[TranscriptHydrationService.BuildAsrScoringView]]
- [[TranscriptHydrationService.BuildBookScoringView]]
- [[ScriptValidator.CalculateSegmentWER]]
- [[ScriptValidator.ExtractWordsFromScript]]
- [[TextNormalizerTests.TokenizeWords_ShouldTokenizeCorrectly]]


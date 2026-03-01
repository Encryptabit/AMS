---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Common/TextNormalizer.cs"
access_modifier: "public"
complexity: 2
fan_in: 7
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# TextNormalizer::TokenizeWords
**Path**: `Projects/AMS/host/Ams.Core/Common/TextNormalizer.cs`

## Summary
**Splits text into whitespace-delimited word tokens with an empty-array fallback for blank input.**

`TokenizeWords` returns `Array.Empty<string>()` when the input is null/whitespace, avoiding null propagation and allocations for empty cases. For non-empty input, it tokenizes by a literal space (`' '`) using `StringSplitOptions.RemoveEmptyEntries`, so repeated spaces are collapsed and no empty tokens are produced. It performs no normalization or punctuation handling itself.


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


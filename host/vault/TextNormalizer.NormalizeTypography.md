---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Common/TextNormalizer.cs"
access_modifier: "public"
complexity: 2
fan_in: 6
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# TextNormalizer::NormalizeTypography
**Path**: `Projects/AMS/host/Ams.Core/Common/TextNormalizer.cs`

## Summary
**Converts typographic Unicode punctuation in a string to ASCII-equivalent characters for consistent downstream text processing.**

`NormalizeTypography` short-circuits null/empty input to `string.Empty`, then performs a deterministic character-level normalization chain via successive `string.Replace` calls. It maps multiple Unicode apostrophe variants to ASCII `'`, multiple quote variants to `"`, and common dash/minus variants (`\u2013`, `\u2014`, `\u2212`) to `-`. The method does not tokenize or regex-process text; it only canonicalizes typography glyphs.


#### [[TextNormalizer.NormalizeTypography]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static string NormalizeTypography(string text)
```

**Called-by <-**
- [[TextNormalizer.Normalize]]
- [[BookParser.SplitPdfSentences]]
- [[PronunciationHelper.ExtractPronunciationParts]]
- [[BookIndexer.NormalizeTokenSurface]]
- [[TranscriptHydrationService.NormalizeSurface]]
- [[TextNormalizerTests.NormalizeTypography_ReplacesSmartQuotes]]


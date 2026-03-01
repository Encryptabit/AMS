---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Common/TextNormalizer.cs"
access_modifier: "public"
complexity: 2
fan_in: 6
fan_out: 0
tags:
  - method
---
# TextNormalizer::NormalizeTypography
**Path**: `home/cari/repos/AMS/host/Ams.Core/Common/TextNormalizer.cs`


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


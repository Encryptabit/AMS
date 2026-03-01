---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs"
access_modifier: "private"
complexity: 3
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# TranscriptHydrationService::NormalizeSurface
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs`

## Summary
**Normalizes a text fragment into a trimmed, typography-cleaned surface or an empty string when no meaningful content exists.**

`NormalizeSurface` canonicalizes text surfaces for diffing/join operations with defensive empty handling. It returns `string.Empty` for null/whitespace input, otherwise applies `TextNormalizer.NormalizeTypography(text)` and trims the result; if normalization yields only whitespace, it still returns `string.Empty`. This ensures callers receive a stable non-null normalized string.


#### [[TranscriptHydrationService.NormalizeSurface]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string NormalizeSurface(string text)
```

**Calls ->**
- [[TextNormalizer.NormalizeTypography]]

**Called-by <-**
- [[TranscriptHydrationService.JoinAsr]]
- [[TranscriptHydrationService.JoinBook]]


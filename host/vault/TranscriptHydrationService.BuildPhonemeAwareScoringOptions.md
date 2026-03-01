---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/factory
---
# TranscriptHydrationService::BuildPhonemeAwareScoringOptions
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs`

## Summary
**Builds diff-scoring options that enable exact phoneme-aware comparison between reference and hypothesis token views.**

`BuildPhonemeAwareScoringOptions` is a pure projection helper that packages token and phoneme arrays into `TextDiffScoringOptions`. It copies `referenceView` and `hypothesisView` tokens/phoneme variants into the corresponding reference/hypothesis fields and hard-codes `UseExactPhonemeEquivalence: true`. No validation or transformation is applied beyond field mapping.


#### [[TranscriptHydrationService.BuildPhonemeAwareScoringOptions]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static TextDiffScoringOptions BuildPhonemeAwareScoringOptions(TranscriptHydrationService.TokenPhonemeView referenceView, TranscriptHydrationService.TokenPhonemeView hypothesisView)
```

**Called-by <-**
- [[TranscriptHydrationService.BuildHydratedTranscriptAsync]]


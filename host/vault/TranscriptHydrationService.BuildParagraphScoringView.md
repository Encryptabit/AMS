---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/factory
  - llm/validation
---
# TranscriptHydrationService::BuildParagraphScoringView
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs`

## Summary
**Builds a paragraph-scoring token/phoneme view by concatenating sentence-scoring views for the paragraph.**

`BuildParagraphScoringView` aggregates sentence-level `TokenPhonemeView` entries into a paragraph-level view in sentence-ID order. It returns `EmptyTokenPhonemeView` when `sentenceIds` is empty, otherwise estimates capacity by summing token counts from available sentence views, preallocates token/phoneme lists, and appends `view.Tokens`/`view.Phonemes` for each resolvable non-empty sentence. Missing sentence IDs or empty views are skipped. The method returns a new combined `TokenPhonemeView(tokens, phonemes)`.


#### [[TranscriptHydrationService.BuildParagraphScoringView]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static TranscriptHydrationService.TokenPhonemeView BuildParagraphScoringView(IReadOnlyList<int> sentenceIds, IReadOnlyDictionary<int, TranscriptHydrationService.TokenPhonemeView> sentenceScoringViews)
```

**Called-by <-**
- [[TranscriptHydrationService.BuildHydratedTranscriptAsync]]


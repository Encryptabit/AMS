---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 12
tags:
  - method
  - llm/async
  - llm/factory
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# TranscriptHydrationService::BuildHydratedTranscriptAsync
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs`

## Summary
**Constructs a fully enriched hydrated transcript by combining transcript alignments with book/ASR text, phoneme-aware diffing, and derived quality metrics.**

`BuildHydratedTranscriptAsync` performs the full transcript hydration pipeline by requiring loaded `BookIndex` and ASR artifacts, computing pronunciation fallbacks, and projecting transcript words/sentences/paragraphs into enriched hydrated models. It builds phoneme-aware scoring views (`BuildBookScoringView`/`BuildAsrScoringView`/`BuildParagraphScoringView`), runs `TextDiffAnalyzer.Analyze(...)` for sentence and paragraph comparisons, derives statuses (`ResolveSentenceStatus`, `ResolveParagraphStatus`), and assembles metrics/diff payloads. It materializes `HydratedWord`, `HydratedSentence`, and `HydratedParagraph` collections, then returns a new `HydratedTranscript` containing those projections plus source metadata. Missing book/ASR prerequisites throw `InvalidOperationException`.


#### [[TranscriptHydrationService.BuildHydratedTranscriptAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<HydratedTranscript> BuildHydratedTranscriptAsync(ChapterContext context, TranscriptIndex transcript, CancellationToken cancellationToken)
```

**Calls ->**
- [[AsrResponse.GetWord]]
- [[TextDiffAnalyzer.Analyze_2]]
- [[TranscriptHydrationService.BuildAsrScoringView]]
- [[TranscriptHydrationService.BuildBookScoringView]]
- [[TranscriptHydrationService.BuildParagraphScoringView]]
- [[TranscriptHydrationService.BuildParagraphScript]]
- [[TranscriptHydrationService.BuildPhonemeAwareScoringOptions]]
- [[TranscriptHydrationService.BuildPronunciationFallbackAsync]]
- [[TranscriptHydrationService.JoinAsr]]
- [[TranscriptHydrationService.JoinBook]]
- [[TranscriptHydrationService.ResolveParagraphStatus]]
- [[TranscriptHydrationService.ResolveSentenceStatus]]

**Called-by <-**
- [[TranscriptHydrationService.HydrateTranscriptAsync]]


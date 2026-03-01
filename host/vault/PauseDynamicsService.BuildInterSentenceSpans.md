---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "private"
complexity: 12
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# PauseDynamicsService::BuildInterSentenceSpans
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`

## Summary
**Constructs inter-sentence pause spans and classifies them by structural boundaries for pause analysis.**

`BuildInterSentenceSpans` sorts transcript sentences by start/end time, then walks adjacent sentence pairs to derive pause gaps from `left.Timing.EndSec` to `right.Timing.StartSec`, skipping non-finite or non-positive gaps. It resolves paragraph IDs via `sentenceToParagraph` to classify each gap as `PauseClass.Paragraph` when crossing paragraphs, otherwise `PauseClass.Sentence`, and marks `CrossesChapterHead` when either side belongs to `headingParagraphIds`. Each valid gap is emitted as a `PauseSpan` with `HasGapHint: false` and `Provenance: ScriptPunctuation`. A comma-span debug log branch exists but is effectively dormant here because this method only produces sentence/paragraph classes.


#### [[PauseDynamicsService.BuildInterSentenceSpans]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<PauseSpan> BuildInterSentenceSpans(TranscriptIndex transcript, IReadOnlyDictionary<int, int> sentenceToParagraph, HashSet<int> headingParagraphIds)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[PauseDynamicsService.AnalyzeChapter]]


---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# TranscriptIndexService::BuildRollups
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs`

## Summary
**Builds section-scoped sentence and paragraph alignment rollups from word-level operations.**

This helper computes sentence/paragraph rollup inputs within an effective book section, then delegates aggregation to `TranscriptAligner.Rollup`. If `pipeline.Section` exists it clamps to that range; otherwise it derives `[secStartWord, secEndWord]` from aligned book indices in `wordOps` (excluding deletions) plus `anchorOps`, expands to enclosing sentence boundaries, and re-clamps to book bounds. It then selects only overlapping sentences/paragraphs, clips each tuple to the section window, and passes these ranges with `wordOps`, `book`, and `asr` into `Rollup`.


#### [[TranscriptIndexService.BuildRollups]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static (IReadOnlyList<SentenceAlign> Sentences, IReadOnlyList<ParagraphAlign> Paragraphs) BuildRollups(BookIndex book, AsrResponse asr, AnchorPipelineResult pipeline, IReadOnlyList<WordAlign> wordOps, IReadOnlyList<WordAlign> anchorOps)
```

**Calls ->**
- [[TranscriptAligner.Rollup_2]]

**Called-by <-**
- [[TranscriptIndexService.BuildTranscriptIndexAsync]]


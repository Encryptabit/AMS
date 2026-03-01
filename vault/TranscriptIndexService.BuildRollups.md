---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 1
tags:
  - method
---
# TranscriptIndexService::BuildRollups
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs`


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


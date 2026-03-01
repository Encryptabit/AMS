---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs"
access_modifier: "private"
complexity: 12
fan_in: 1
fan_out: 2
tags:
  - method
---
# TranscriptIndexService::BuildWordOperations
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs`


#### [[TranscriptIndexService.BuildWordOperations]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static (List<WordAlign> WordOps, List<WordAlign> AnchorOps) BuildWordOperations(AnchorPipelineResult pipeline, AnchorPolicy policy, BookIndex book, AsrAnchorView asrView, IReadOnlyList<(int bLo, int bHi, int aLo, int aHi)> windows, string[][] bookPhonemes, string[][] asrPhonemes)
```

**Calls ->**
- [[AnchorPreprocessor.BuildBookView]]
- [[TranscriptAligner.AlignWindows]]

**Called-by <-**
- [[TranscriptIndexService.BuildTranscriptIndexAsync]]


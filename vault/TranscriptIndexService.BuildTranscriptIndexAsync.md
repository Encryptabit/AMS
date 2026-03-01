---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs"
access_modifier: "public"
complexity: 12
fan_in: 0
fan_out: 16
tags:
  - method
---
# TranscriptIndexService::BuildTranscriptIndexAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs`


#### [[TranscriptIndexService.BuildTranscriptIndexAsync]]
##### What it does:
<member name="M:Ams.Core.Services.Alignment.TranscriptIndexService.BuildTranscriptIndexAsync(Ams.Core.Runtime.Chapter.ChapterContext,Ams.Core.Services.Alignment.TranscriptBuildOptions,System.Threading.CancellationToken)">
    <inheritdoc />
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<TranscriptIndex> BuildTranscriptIndexAsync(ChapterContext context, TranscriptBuildOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[AnchorPipeline.ComputeAnchors]]
- [[AnchorPreprocessor.BuildAsrView]]
- [[AnchorPreprocessor.BuildBookView]]
- [[ChapterContext.GetOrResolveSection]]
- [[ChapterContext.SetDetectedSection]]
- [[TranscriptIndexService.BuildAnchorDocument]]
- [[TranscriptIndexService.BuildAsrPhonemeViewAsync]]
- [[TranscriptIndexService.BuildBookPhonemeView]]
- [[TranscriptIndexService.BuildFallbackWindows]]
- [[TranscriptIndexService.BuildPolicy]]
- [[TranscriptIndexService.BuildRollups]]
- [[TranscriptIndexService.BuildWordOperations]]
- [[TranscriptIndexService.ComputeTiming]]
- [[TranscriptIndexService.RequireBookAndAsr]]
- [[TranscriptIndexService.ResolveDefaultAudioPath]]
- [[TranscriptIndexService.ResolveDefaultBookIndexPath]]


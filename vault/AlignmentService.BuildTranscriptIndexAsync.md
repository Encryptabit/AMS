---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Alignment/AlignmentService.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
---
# AlignmentService::BuildTranscriptIndexAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Alignment/AlignmentService.cs`


#### [[AlignmentService.BuildTranscriptIndexAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<TranscriptIndex> BuildTranscriptIndexAsync(ChapterContext context, TranscriptBuildOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[ITranscriptIndexService.BuildTranscriptIndexAsync]]


---
namespace: "Ams.Core.Services.Interfaces"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Interfaces/IAlignmentService.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
---
# IAlignmentService::BuildTranscriptIndexAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Interfaces/IAlignmentService.cs`


#### [[IAlignmentService.BuildTranscriptIndexAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
Task<TranscriptIndex> BuildTranscriptIndexAsync(ChapterContext context, TranscriptBuildOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
```

**Called-by <-**
- [[BuildTranscriptIndexCommand.ExecuteAsync]]


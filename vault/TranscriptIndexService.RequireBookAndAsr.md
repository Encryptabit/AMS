---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
---
# TranscriptIndexService::RequireBookAndAsr
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs`


#### [[TranscriptIndexService.RequireBookAndAsr]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static (BookIndex Book, AsrResponse Asr) RequireBookAndAsr(ChapterContext context)
```

**Called-by <-**
- [[TranscriptIndexService.BuildTranscriptIndexAsync]]


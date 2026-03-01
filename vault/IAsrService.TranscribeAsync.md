---
namespace: "Ams.Core.Services.Interfaces"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Interfaces/IAsrService.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
---
# IAsrService::TranscribeAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Interfaces/IAsrService.cs`


#### [[IAsrService.TranscribeAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
Task<AsrResponse> TranscribeAsync(ChapterContext chapter, AsrOptions options, CancellationToken cancellationToken = default(CancellationToken))
```

**Called-by <-**
- [[GenerateTranscriptCommand.RunWhisperAsync]]


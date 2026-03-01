---
namespace: "Ams.Core.Services"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/AsrService.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 2
tags:
  - method
---
# AsrService::TranscribeAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/AsrService.cs`


#### [[AsrService.TranscribeAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<AsrResponse> TranscribeAsync(ChapterContext chapter, AsrOptions options, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[AsrProcessor.TranscribeBufferAsync]]
- [[AsrService.ResolveAsrReadyBuffer]]


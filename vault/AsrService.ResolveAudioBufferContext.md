---
namespace: "Ams.Core.Services"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/AsrService.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
---
# AsrService::ResolveAudioBufferContext
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/AsrService.cs`


#### [[AsrService.ResolveAudioBufferContext]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static AudioBufferContext ResolveAudioBufferContext(ChapterContext chapter)
```

**Calls ->**
- [[AudioBufferManager.Load_2]]

**Called-by <-**
- [[AsrService.ResolveAsrReadyBuffer]]


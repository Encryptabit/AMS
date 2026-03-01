---
namespace: "Ams.Core.Services"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/AsrService.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 2
tags:
  - method
---
# AsrService::ResolveAsrReadyBuffer
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/AsrService.cs`


#### [[AsrService.ResolveAsrReadyBuffer]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public AudioBuffer ResolveAsrReadyBuffer(ChapterContext chapter)
```

**Calls ->**
- [[AsrAudioPreparer.PrepareForAsr]]
- [[AsrService.ResolveAudioBufferContext]]

**Called-by <-**
- [[AsrService.TranscribeAsync]]


---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "public"
complexity: 6
fan_in: 1
fan_out: 3
tags:
  - method
---
# AudioAccumulator::ToBuffer
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`


#### [[AudioAccumulator.ToBuffer]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public AudioBuffer ToBuffer(AudioBufferMetadata templateMetadata = null)
```

**Calls ->**
- [[AudioBufferMetadata.CreateDefault]]
- [[AudioBufferMetadata.DescribeDefaultLayout]]
- [[AudioBufferMetadata.WithCurrentStream]]

**Called-by <-**
- [[FilterGraphExecutor.BuildOutput]]


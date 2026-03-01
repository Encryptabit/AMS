---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/AudioProcessor.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
---
# AudioProcessor::EncodeWavToStream
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/AudioProcessor.cs`


#### [[AudioProcessor.EncodeWavToStream]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static MemoryStream EncodeWavToStream(AudioBuffer buffer, AudioEncodeOptions options = null)
```

**Calls ->**
- [[FfEncoder.EncodeToCustomStream]]

**Called-by <-**
- [[AudioBuffer.ToWavStream]]


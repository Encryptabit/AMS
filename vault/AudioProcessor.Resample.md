---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/AudioProcessor.cs"
access_modifier: "public"
complexity: 4
fan_in: 3
fan_out: 3
tags:
  - method
---
# AudioProcessor::Resample
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/AudioProcessor.cs`


#### [[AudioProcessor.Resample]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AudioBuffer Resample(AudioBuffer buffer, ulong targetSampleRate)
```

**Calls ->**
- [[FfFilterGraph.FromBuffer]]
- [[FfFilterGraph.Resample]]
- [[FfFilterGraph.ToBuffer]]

**Called-by <-**
- [[AsrAudioPreparer.PrepareForAsr]]
- [[AudioSpliceService.InsertAtPoint]]
- [[AudioSpliceService.ReplaceSegment]]


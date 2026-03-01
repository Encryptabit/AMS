---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/AudioProcessor.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
---
# AudioProcessor::Probe
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/AudioProcessor.cs`


#### [[AudioProcessor.Probe]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AudioInfo Probe(string path)
```

**Calls ->**
- [[FfDecoder.Probe]]

**Called-by <-**
- [[AudioController.GetChapterRegionAudio]]


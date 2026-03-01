---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/AudioProcessor.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 3
tags:
  - method
---
# AudioProcessor::FadeIn
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/AudioProcessor.cs`


#### [[AudioProcessor.FadeIn]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AudioBuffer FadeIn(AudioBuffer buffer, TimeSpan duration)
```

**Calls ->**
- [[FfFilterGraph.Custom]]
- [[FfFilterGraph.FromBuffer]]
- [[FfFilterGraph.ToBuffer]]

**Called-by <-**
- [[AudioProcessorFilterTests.FadeIn_GraduallyIncreasesAmplitude]]


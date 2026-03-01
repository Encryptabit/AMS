---
namespace: "Ams.Tests"
project: "Ams.Tests"
source_file: "home/cari/repos/AMS/host/Ams.Tests/AudioProcessorFilterTests.cs"
access_modifier: "private"
complexity: 4
fan_in: 3
fan_out: 0
tags:
  - method
---
# AudioProcessorFilterTests::CreateBuffer
**Path**: `home/cari/repos/AMS/host/Ams.Tests/AudioProcessorFilterTests.cs`


#### [[AudioProcessorFilterTests.CreateBuffer]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static AudioBuffer CreateBuffer(params (double frequency, double seconds)[] segments)
```

**Called-by <-**
- [[AudioProcessorFilterTests.DetectSilence_FindsInitialGap]]
- [[AudioProcessorFilterTests.FadeIn_GraduallyIncreasesAmplitude]]
- [[AudioProcessorFilterTests.Trim_ReturnsExpectedSegment]]


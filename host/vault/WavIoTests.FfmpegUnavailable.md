---
namespace: "Ams.Tests"
project: "Ams.Tests"
source_file: "Projects/AMS/host/Ams.Tests/WavIoTests.cs"
access_modifier: "private"
complexity: 2
fan_in: 4
fan_out: 1
tags:
  - method
---
# WavIoTests::FfmpegUnavailable
**Path**: `Projects/AMS/host/Ams.Tests/WavIoTests.cs`


#### [[WavIoTests.FfmpegUnavailable]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool FfmpegUnavailable()
```

**Calls ->**
- [[FfSession.EnsureInitialized]]

**Called-by <-**
- [[WavIoTests.ReadFloat32_PreservesValues]]
- [[WavIoTests.ReadPcm16_SucceedsWithOddSizedMetadataChunk]]
- [[WavIoTests.ReadPcm24_ConvertsSamplesToFloat]]
- [[WavIoTests.ReadPcm32_ConvertsSamplesToFloat]]


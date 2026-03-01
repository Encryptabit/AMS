---
namespace: "Ams.Tests"
project: "Ams.Tests"
source_file: "home/cari/repos/AMS/host/Ams.Tests/WavIoTests.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 3
tags:
  - method
---
# WavIoTests::ReadFloat32_PreservesValues
**Path**: `home/cari/repos/AMS/host/Ams.Tests/WavIoTests.cs`


#### [[WavIoTests.ReadFloat32_PreservesValues]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void ReadFloat32_PreservesValues()
```

**Calls ->**
- [[AudioProcessor.Decode]]
- [[WavIoTests.FfmpegUnavailable]]
- [[WavIoTests.WriteTempWav]]


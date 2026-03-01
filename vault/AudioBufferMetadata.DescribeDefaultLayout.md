---
namespace: "Ams.Core.Artifacts"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Artifacts/AudioBufferMetadata.cs"
access_modifier: "public"
complexity: 9
fan_in: 6
fan_out: 0
tags:
  - method
---
# AudioBufferMetadata::DescribeDefaultLayout
**Path**: `home/cari/repos/AMS/host/Ams.Core/Artifacts/AudioBufferMetadata.cs`


#### [[AudioBufferMetadata.DescribeDefaultLayout]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static string DescribeDefaultLayout(int channels)
```

**Called-by <-**
- [[AudioBufferMetadata.CreateDefault]]
- [[AudioBufferMetadata.WithCurrentStream]]
- [[FfDecoder.Decode]]
- [[AudioAccumulator.ToBuffer]]
- [[FilterGraphExecutor.ConfigureChannelLayouts]]
- [[FilterGraphExecutor.SetupSource]]


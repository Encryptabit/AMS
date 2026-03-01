---
namespace: "Ams.Core.Artifacts"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Artifacts/AudioBufferMetadata.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
---
# AudioBufferMetadata::WithCurrentStream
**Path**: `home/cari/repos/AMS/host/Ams.Core/Artifacts/AudioBufferMetadata.cs`


#### [[AudioBufferMetadata.WithCurrentStream]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public AudioBufferMetadata WithCurrentStream(int sampleRate, int channels, string sampleFormat, string channelLayout)
```

**Calls ->**
- [[AudioBufferMetadata.DescribeDefaultLayout]]

**Called-by <-**
- [[AudioAccumulator.ToBuffer]]


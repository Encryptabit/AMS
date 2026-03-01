---
namespace: "Ams.Core.Artifacts"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Artifacts/AudioBufferMetadata.cs"
access_modifier: "public"
complexity: 1
fan_in: 2
fan_out: 1
tags:
  - method
---
# AudioBufferMetadata::CreateDefault
**Path**: `home/cari/repos/AMS/host/Ams.Core/Artifacts/AudioBufferMetadata.cs`


#### [[AudioBufferMetadata.CreateDefault]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AudioBufferMetadata CreateDefault(int sampleRate, int channels)
```

**Calls ->**
- [[AudioBufferMetadata.DescribeDefaultLayout]]

**Called-by <-**
- [[AudioBuffer..ctor]]
- [[AudioAccumulator.ToBuffer]]


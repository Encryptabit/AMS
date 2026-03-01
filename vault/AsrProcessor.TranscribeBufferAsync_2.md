---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 2
tags:
  - method
---
# AsrProcessor::TranscribeBufferAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs`


#### [[AsrProcessor.TranscribeBufferAsync_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Task<AsrResponse> TranscribeBufferAsync(ReadOnlyMemory<float> monoAudio, AsrOptions options, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[AsrProcessor.EnsureModelPath]]
- [[AsrProcessor.TranscribeBufferInternalAsync]]


---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 3
tags:
  - method
---
# AsrProcessor::TranscribeFileAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs`


#### [[AsrProcessor.TranscribeFileAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Task<AsrResponse> TranscribeFileAsync(string audioPath, AsrOptions options, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[AsrProcessor.EnsureModelPath]]
- [[AsrProcessor.TranscribeBufferInternalAsync]]
- [[AudioProcessor.Decode]]


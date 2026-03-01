---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 3
tags:
  - method
---
# AsrProcessor::TranscribeWithWhisperNetAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs`


#### [[AsrProcessor.TranscribeWithWhisperNetAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task<AsrResponse> TranscribeWithWhisperNetAsync(AudioBuffer buffer, AsrOptions options, CancellationToken cancellationToken)
```

**Calls ->**
- [[Log.Warn]]
- [[AsrProcessor.RunWhisperPassAsync]]
- [[AsrProcessor.ShouldRetryWithoutDtw]]

**Called-by <-**
- [[AsrProcessor.TranscribeBufferInternalAsync]]


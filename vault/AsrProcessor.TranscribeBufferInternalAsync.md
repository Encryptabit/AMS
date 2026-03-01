---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 1
fan_in: 3
fan_out: 2
tags:
  - method
---
# AsrProcessor::TranscribeBufferInternalAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs`


#### [[AsrProcessor.TranscribeBufferInternalAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task<AsrResponse> TranscribeBufferInternalAsync(AudioBuffer buffer, AsrOptions options, CancellationToken cancellationToken)
```

**Calls ->**
- [[AsrAudioPreparer.PrepareForAsr]]
- [[AsrProcessor.TranscribeWithWhisperNetAsync]]

**Called-by <-**
- [[AsrProcessor.TranscribeBufferAsync]]
- [[AsrProcessor.TranscribeBufferAsync_2]]
- [[AsrProcessor.TranscribeFileAsync]]


---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 6
tags:
  - method
---
# AsrProcessor::RunWhisperPassAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs`


#### [[AsrProcessor.RunWhisperPassAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task<AsrResponse> RunWhisperPassAsync(AudioBuffer buffer, AsrOptions options, CancellationToken cancellationToken)
```

**Calls ->**
- [[AudioBuffer.ToWavStream]]
- [[Log.Debug]]
- [[AsrProcessor.AppendTokens]]
- [[AsrProcessor.ConfigureBuilder]]
- [[AsrProcessor.CreateFactoryOptions]]
- [[WhisperFactoryPool.Acquire]]

**Called-by <-**
- [[AsrProcessor.TranscribeWithWhisperNetAsync]]


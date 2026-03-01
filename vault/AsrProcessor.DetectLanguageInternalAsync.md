---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 2
fan_in: 0
fan_out: 4
tags:
  - method
---
# AsrProcessor::DetectLanguageInternalAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs`


#### [[AsrProcessor.DetectLanguageInternalAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task<string> DetectLanguageInternalAsync(AudioBuffer buffer, AsrOptions options, CancellationToken cancellationToken)
```

**Calls ->**
- [[AsrProcessor.ConfigureBuilder]]
- [[AsrProcessor.CreateFactoryOptions]]
- [[AsrProcessor.ExtractMonoSamples]]
- [[WhisperFactoryPool.Acquire]]


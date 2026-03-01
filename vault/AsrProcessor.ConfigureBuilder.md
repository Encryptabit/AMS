---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 1
fan_in: 2
fan_out: 1
tags:
  - method
---
# AsrProcessor::ConfigureBuilder
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs`


#### [[AsrProcessor.ConfigureBuilder]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static WhisperProcessorBuilder ConfigureBuilder(WhisperFactory factory, AsrOptions options, bool enableTokenTimestamps)
```

**Calls ->**
- [[AsrProcessor.ConfigureBuilder_2]]

**Called-by <-**
- [[AsrProcessor.DetectLanguageInternalAsync]]
- [[AsrProcessor.RunWhisperPassAsync]]


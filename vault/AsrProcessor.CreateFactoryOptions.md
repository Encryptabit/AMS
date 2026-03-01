---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 7
fan_in: 2
fan_out: 2
tags:
  - method
---
# AsrProcessor::CreateFactoryOptions
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs`


#### [[AsrProcessor.CreateFactoryOptions]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static WhisperFactoryOptions CreateFactoryOptions(AsrOptions options)
```

**Calls ->**
- [[Log.Warn]]
- [[AsrProcessor.ResolveDtwPreset]]

**Called-by <-**
- [[AsrProcessor.DetectLanguageInternalAsync]]
- [[AsrProcessor.RunWhisperPassAsync]]


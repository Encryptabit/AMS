---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Asr/AsrEngine.cs"
access_modifier: "public"
complexity: 7
fan_in: 6
fan_out: 0
tags:
  - method
---
# AsrEngineConfig::Resolve
**Path**: `home/cari/repos/AMS/host/Ams.Core/Asr/AsrEngine.cs`


#### [[AsrEngineConfig.Resolve]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AsrEngine Resolve(string engineOption = null)
```

**Called-by <-**
- [[AsrCommand.Create]]
- [[PipelineCommand.RunPipelineAsync]]
- [[Program.Main]]
- [[GenerateTranscriptCommand.ExecuteAsync]]
- [[AsrEngineConfig.IsNemo]]
- [[AsrEngineConfig.IsWhisper]]


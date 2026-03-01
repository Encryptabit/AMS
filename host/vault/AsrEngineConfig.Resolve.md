---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Asr/AsrEngine.cs"
access_modifier: "public"
complexity: 7
fan_in: 6
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AsrEngineConfig::Resolve
**Path**: `Projects/AMS/host/Ams.Core/Asr/AsrEngine.cs`

## Summary
**Resolve the effective ASR engine selection from CLI/config input or environment with Whisper as the default/fallback.**

`Resolve` determines the ASR engine from an explicit `engineOption` or, if null, from `AMS_ASR_ENGINE`. It defaults to `AsrEngine.Whisper` when the input is null/whitespace, then normalizes with `Trim().ToLowerInvariant()` and maps known tokens (`"nemo"`, `"whisper"`, `"whispernet"`, `"whisper.net"`) via a switch expression. Any unrecognized value also falls back to `Whisper`, making resolution tolerant and non-throwing.


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


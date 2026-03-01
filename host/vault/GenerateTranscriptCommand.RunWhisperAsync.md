---
namespace: "Ams.Core.Application.Commands"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 4
tags:
  - method
---
# GenerateTranscriptCommand::RunWhisperAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs`

## Summary
****

`RunWhisperAsync(...)` is the Whisper-specific execution path called by `ExecuteAsync` that logs diagnostics (`Debug`), resolves the Whisper model location (`ResolveModelPathAsync`), runs transcription via `TranscribeAsync`, and persists the raw/result payload through `PersistResponse` using the supplied chapter/options/token context.


#### [[GenerateTranscriptCommand.RunWhisperAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task RunWhisperAsync(ChapterContext chapter, GenerateTranscriptOptions options, CancellationToken cancellationToken)
```

**Calls ->**
- [[GenerateTranscriptCommand.PersistResponse]]
- [[AsrEngineConfig.ResolveModelPathAsync]]
- [[Log.Debug]]
- [[IAsrService.TranscribeAsync]]

**Called-by <-**
- [[GenerateTranscriptCommand.ExecuteAsync]]


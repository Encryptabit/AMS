---
namespace: "Ams.Core.Application.Commands"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 8
tags:
  - method
  - llm/async
  - llm/di
  - llm/validation
  - llm/error-handling
---
# GenerateTranscriptCommand::RunNemoAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs`

## Summary
**It prepares chapter audio for Nemo ASR, verifies service readiness/health, submits transcription, stores the response, and cleans up temporary artifacts.**

`RunNemoAsync` resolves the Nemo endpoint from `options.ServiceUrl` (falling back to `GenerateTranscriptOptions.DefaultServiceUrl`), logs it, and awaits `AsrProcessSupervisor.EnsureServiceReadyAsync(...)` before any request work. It pulls an ASR-ready audio buffer from `_asrService.ResolveAsrReadyBuffer(chapter)`, exports it to a temp WAV via `ExportBufferToTempFile`, then creates an `AsrClient` to run `IsHealthyAsync` and fail fast with `InvalidOperationException` if unhealthy. On success it calls `TranscribeAsync(...)`, persists results through `PersistResponse`, and guarantees temp-file cleanup in `finally` via `TryDelete`.


#### [[GenerateTranscriptCommand.RunNemoAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task RunNemoAsync(ChapterContext chapter, GenerateTranscriptOptions options, CancellationToken cancellationToken)
```

**Calls ->**
- [[GenerateTranscriptCommand.ExportBufferToTempFile]]
- [[GenerateTranscriptCommand.PersistResponse]]
- [[GenerateTranscriptCommand.TryDelete]]
- [[AsrProcessSupervisor.EnsureServiceReadyAsync]]
- [[AsrClient.IsHealthyAsync]]
- [[AsrClient.TranscribeAsync]]
- [[Log.Debug]]
- [[IAsrService.ResolveAsrReadyBuffer]]

**Called-by <-**
- [[GenerateTranscriptCommand.ExecuteAsync]]


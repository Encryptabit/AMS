---
namespace: "Ams.Core.Application.Commands"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 8
tags:
  - method
---
# GenerateTranscriptCommand::RunNemoAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs`


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


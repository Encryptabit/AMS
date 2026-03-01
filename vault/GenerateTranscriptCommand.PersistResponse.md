---
namespace: "Ams.Core.Application.Commands"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 2
fan_out: 2
tags:
  - method
---
# GenerateTranscriptCommand::PersistResponse
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs`


#### [[GenerateTranscriptCommand.PersistResponse]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void PersistResponse(ChapterContext chapter, AsrResponse response)
```

**Calls ->**
- [[AsrTranscriptBuilder.BuildCorpusText]]
- [[Log.Debug]]

**Called-by <-**
- [[GenerateTranscriptCommand.RunNemoAsync]]
- [[GenerateTranscriptCommand.RunWhisperAsync]]


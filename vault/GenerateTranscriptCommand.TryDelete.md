---
namespace: "Ams.Core.Application.Commands"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
---
# GenerateTranscriptCommand::TryDelete
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs`


#### [[GenerateTranscriptCommand.TryDelete]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void TryDelete(FileInfo file)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[GenerateTranscriptCommand.RunNemoAsync]]


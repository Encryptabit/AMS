---
namespace: "Ams.Core.Application.Commands"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# GenerateTranscriptCommand::TryDelete
**Path**: `Projects/AMS/host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs`

## Summary
**It attempts to delete a file without propagating IO errors, logging failures at debug level.**

`TryDelete` performs best-effort cleanup of a temporary file by first checking `file.Exists` and then calling `file.Delete()` inside a `try/catch`. Any deletion failure is intentionally suppressed, with diagnostics emitted via `Log.Debug` including `file.FullName` and the exception message. This keeps cleanup non-fatal for the caller (`RunNemoAsync` finally block).


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


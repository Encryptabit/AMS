---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/AlignCommand.cs"
access_modifier: "private"
complexity: 4
fan_in: 3
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
---
# AlignCommand::CopyIfRequested
**Path**: `Projects/AMS/host/Ams.Cli/Commands/AlignCommand.cs`

## Summary
**Conditionally persist a generated artifact to an optional output path by creating parent folders, overwriting the file, and logging the write.**

`CopyIfRequested` is a small filesystem helper that no-ops when `destination` is `null`, then ensures the destination directory exists via `Directory.CreateDirectory(destination.Directory?.FullName ?? Directory.GetCurrentDirectory())`. It copies `source.FullName` to `destination.FullName` with `overwrite: true`, so repeated command runs replace prior output. After copying, it emits a debug log (`Log.Debug("Wrote {Path}", destination.FullName)`) for traceability from the calling command handlers.


#### [[AlignCommand.CopyIfRequested]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void CopyIfRequested(FileInfo source, FileInfo destination)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[AlignCommand.CreateAnchors]]
- [[AlignCommand.CreateHydrateTx]]
- [[AlignCommand.CreateTranscriptIndex]]


---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/error-handling
---
# DspCommand::TryDeleteDirectory
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Attempts to recursively delete a directory path during DSP chain cleanup without letting deletion errors fail the command.**

`TryDeleteDirectory` is a best-effort filesystem cleanup helper that checks `Directory.Exists(path)` and then calls `Directory.Delete(path, recursive: true)`. It wraps deletion in a broad `try/catch (Exception)` and intentionally suppresses failures, emitting only `Log.Debug("Failed to delete temporary directory {Path}", path)` without rethrowing. In `RunChainAsync`, it is used from the `finally` cleanup path to remove the generated temp work root when no explicit work directory is provided.


#### [[DspCommand.TryDeleteDirectory]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void TryDeleteDirectory(string path)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[DspCommand.RunChainAsync]]


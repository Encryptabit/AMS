---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# ValidateCommand::ResolveValidationViewerScript
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`

## Summary
**Find the validation viewer Python script by checking known relative paths and bounded parent-directory fallbacks.**

`ResolveValidationViewerScript` derives `baseDir` from `AppContext.BaseDirectory`, then uses a local `TryCandidate` helper that returns `Path.GetFullPath(path)` only when `File.Exists(path)` is true. It probes two immediate locations first (`tools/validation-viewer/server.py` and `validation-viewer/server.py`), then performs an upward parent-directory search (max 8 levels) for `tools/validation-viewer/server.py`. The method returns the first resolved absolute path, or `null` when no candidate exists, which `CreateServeCommand` uses to decide whether to start the Python process.


#### [[ValidateCommand.ResolveValidationViewerScript]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ResolveValidationViewerScript()
```

**Calls ->**
- [[TryCandidate]]

**Called-by <-**
- [[ValidateCommand.CreateServeCommand]]


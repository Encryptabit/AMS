---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 4
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# ValidateCommand::MakeAbsolute
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`

## Summary
**Resolve a potentially relative path into an absolute normalized path using a provided base directory.**

`MakeAbsolute` is a path-normalization helper that converts CLI input into a canonical absolute filesystem path. Its logic branches on whether `path` is already rooted; if not, it resolves the relative value against `baseDirectory` and normalizes the result (typically via `Path.Combine`/`Path.GetFullPath`). This keeps downstream callers like `ResolveAudioPath` and `TryResolveBookIndex` working with consistent absolute paths.


#### [[ValidateCommand.MakeAbsolute]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string MakeAbsolute(string path, string baseDirectory)
```

**Called-by <-**
- [[ValidateCommand.ResolveAudioPath]]
- [[ValidateCommand.TryResolveBookIndex]]


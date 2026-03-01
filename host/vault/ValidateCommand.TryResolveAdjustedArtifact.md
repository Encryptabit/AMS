---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/error-handling
---
# ValidateCommand::TryResolveAdjustedArtifact
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`

## Summary
**Resolve and return the best artifact file for validation by trying a suffix-adjusted output path before falling back to the reference file.**

`TryResolveAdjustedArtifact` is a private static helper in `Ams.Cli.Commands.ValidateCommand` that computes a suffix-adjusted artifact path from a reference file by delegating path construction to `BuildOutputJsonPath`. Given its low complexity (2), it likely uses one existence/fallback branch to return either the adjusted `FileInfo` or the original reference. This centralizes artifact path adjustment for `CreateTimingCommand` without duplicating resolution logic.


#### [[ValidateCommand.TryResolveAdjustedArtifact]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static FileInfo TryResolveAdjustedArtifact(FileInfo reference, string suffix)
```

**Calls ->**
- [[ValidateCommand.BuildOutputJsonPath]]

**Called-by <-**
- [[ValidateCommand.CreateTimingCommand]]


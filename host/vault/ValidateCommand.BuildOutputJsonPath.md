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
  - llm/validation
---
# ValidateCommand::BuildOutputJsonPath
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`

## Summary
**Generate a consistent JSON output file path for a reference artifact by combining its base stem with a suffix.**

`BuildOutputJsonPath` is a private static path-construction helper that derives a JSON artifact path from a reference `FileInfo` and a caller-provided suffix. It delegates filename normalization to `GetBaseStem`, then composes a deterministic output name (including the suffix and `.json`) and returns a new `FileInfo` for that path. With complexity 2, its branching is minimal and focused on output-name composition. `TryResolveAdjustedArtifact` relies on it to generate the probe path for adjusted artifacts.


#### [[ValidateCommand.BuildOutputJsonPath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static FileInfo BuildOutputJsonPath(FileInfo reference, string suffix)
```

**Calls ->**
- [[ValidateCommand.GetBaseStem]]

**Called-by <-**
- [[ValidateCommand.TryResolveAdjustedArtifact]]


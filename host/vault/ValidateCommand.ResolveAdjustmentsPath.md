---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 3
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# ValidateCommand::ResolveAdjustmentsPath
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`

## Summary
**Resolves which adjustments file path should be used for validation, preferring an override and falling back to a path derived from the transaction file.**

`ResolveAdjustmentsPath` is a small static resolver that prioritizes an explicit `overrideFile` and otherwise derives the adjustments file path from `txFile` by calling `GetBaseStem` and building a `FileInfo` from that stem. The method is intentionally low-branch (complexity 3), focused on deterministic filename/path selection for validation flow.


#### [[ValidateCommand.ResolveAdjustmentsPath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static FileInfo ResolveAdjustmentsPath(FileInfo txFile, FileInfo overrideFile)
```

**Calls ->**
- [[ValidateCommand.GetBaseStem]]


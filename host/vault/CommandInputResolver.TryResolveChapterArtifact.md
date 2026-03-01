---
namespace: "Ams.Cli.Utilities"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Utilities/CommandInputResolver.cs"
access_modifier: "public"
complexity: 6
fan_in: 3
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# CommandInputResolver::TryResolveChapterArtifact
**Path**: `Projects/AMS/host/Ams.Cli/Utilities/CommandInputResolver.cs`

## Summary
**Resolve a chapter artifact `FileInfo` from user-provided input and suffix, optionally requiring the target file to exist.**

This static helper in `Ams.Cli.Utilities.CommandInputResolver` takes a `FileInfo` input plus an artifact `suffix`, then delegates path derivation to `ResolveChapterFile` to normalize chapter-oriented CLI inputs into a concrete artifact file. With `mustExist` defaulting to `true`, it applies conditional existence/validity checks before returning, which aligns with its moderate cyclomatic complexity (6). It is reused as shared input-resolution logic by `CreateReportCommand`, `CreateTimingCommand`, and `ResolveFilteredOutput`.


#### [[CommandInputResolver.TryResolveChapterArtifact]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static FileInfo TryResolveChapterArtifact(FileInfo provided, string suffix, bool mustExist = true)
```

**Calls ->**
- [[ReplState.ResolveChapterFile]]

**Called-by <-**
- [[DspCommand.ResolveFilteredOutput]]
- [[ValidateCommand.CreateReportCommand]]
- [[ValidateCommand.CreateTimingCommand]]


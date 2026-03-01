---
namespace: "Ams.Cli.Utilities"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Utilities/CommandInputResolver.cs"
access_modifier: "public"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# CommandInputResolver::ResolveOutput
**Path**: `Projects/AMS/host/Ams.Cli/Utilities/CommandInputResolver.cs`

## Summary
**Resolve an output file by using a provided path or deriving one from the active REPL chapter plus a suffix.**

`ResolveOutput` first returns `provided` when the caller supplies an explicit `FileInfo`. If not, it reads `ReplContext.Current`, requires a non-null `ActiveChapterStem`, and throws an `InvalidOperationException` with a CLI-specific message when no active chapter context exists. On success, it delegates to `context.ResolveChapterFile(suffix, mustExist: false)` to synthesize the output path without requiring the file to already exist.


#### [[CommandInputResolver.ResolveOutput]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static FileInfo ResolveOutput(FileInfo provided, string suffix)
```

**Calls ->**
- [[ReplState.ResolveChapterFile]]

**Called-by <-**
- [[DspCommand.ResolveOutputFile]]


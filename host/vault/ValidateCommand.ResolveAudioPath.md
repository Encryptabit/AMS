---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 15
fan_in: 0
fan_out: 4
tags:
  - method
  - danger/high-complexity
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# ValidateCommand::ResolveAudioPath
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`

> [!danger] High Complexity (15)
> Cyclomatic complexity: 15. Consider refactoring into smaller methods.

## Summary
**Resolve and validate a single canonical audio file path from transcript metadata plus related file locations.**

`ResolveAudioPath` in `Ams.Cli.Commands.ValidateCommand` appears to aggregate candidate audio paths from `TranscriptIndex`, `HydratedTranscript`, and the on-disk context of `txFile`/`hydrateFile`. It computes comparable stems via `GetBaseStem` and `NormalizeStem`, canonicalizes candidates with `MakeAbsolute`, and uses `Register(string?, string?)` as a gate to record/validate candidates and handle conflicts. With cyclomatic complexity 15, the implementation likely contains multiple guarded fallback branches for null, mismatched, or ambiguous metadata before returning one resolved string path.


#### [[ValidateCommand.ResolveAudioPath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ResolveAudioPath(TranscriptIndex transcript, HydratedTranscript hydrated, FileInfo txFile, FileInfo hydrateFile)
```

**Calls ->**
- [[ValidateCommand.GetBaseStem]]
- [[ValidateCommand.MakeAbsolute]]
- [[ValidateCommand.NormalizeStem]]
- [[Register]]


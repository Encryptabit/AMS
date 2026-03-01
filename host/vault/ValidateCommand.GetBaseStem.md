---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 5
fan_in: 5
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# ValidateCommand::GetBaseStem
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`

## Summary
**Derive and normalize a consistent base filename stem so downstream validation/path-resolution logic uses the same identifier format.**

`GetBaseStem(string fileName)` is a private static helper in `Ams.Cli.Commands.ValidateCommand` that computes a canonical stem from an input filename and then normalizes it via `NormalizeStem`. With cyclomatic complexity 5, the implementation likely includes several branch paths for filename-shape edge cases (for example, extension/path handling and invalid or degenerate inputs) before returning a stable stem. It is a shared normalization point used by output/report/audio path resolvers and chapter-ID inference (`BuildOutputJsonPath`, `ResolveAdjustmentsPath`, `ResolveAudioPath`, `ResolveDefaultReportPath`, `TryInferChapterId`).


#### [[ValidateCommand.GetBaseStem]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string GetBaseStem(string fileName)
```

**Calls ->**
- [[ValidateCommand.NormalizeStem]]

**Called-by <-**
- [[ValidateCommand.BuildOutputJsonPath]]
- [[ValidateCommand.ResolveAdjustmentsPath]]
- [[ValidateCommand.ResolveAudioPath]]
- [[ValidateCommand.ResolveDefaultReportPath]]
- [[ValidateCommand.TryInferChapterId]]


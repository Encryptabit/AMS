---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 5
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# ValidateCommand::NormalizeStem
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`

## Summary
**Normalize a stem input into a canonical string used consistently by base-stem and audio-path resolution flows.**

The implementation body for `Ams.Cli.Commands.ValidateCommand.NormalizeStem(string)` is not present in the provided workspace/prompt, so statement-level behavior cannot be verified directly; from its call sites (`GetBaseStem`, `ResolveAudioPath`), it functions as the shared stem-canonicalization step before downstream path logic. The reported complexity (5) indicates a compact branch-based normalization routine rather than deep parsing or IO work.


#### [[ValidateCommand.NormalizeStem]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string NormalizeStem(string stem)
```

**Called-by <-**
- [[ValidateCommand.GetBaseStem]]
- [[ValidateCommand.ResolveAudioPath]]


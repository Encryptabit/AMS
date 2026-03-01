---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# PipelineCommand::FormatDb
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Convert a linear audio amplitude into a stable, human-readable dBFS string with correct handling of non-positive values.**

`FormatDb` converts a linear amplitude value to a dBFS display string used by `CreateAudioTable`. It guards `amplitude <= 0` and returns the sentinel `"-∞ dBFS"` to represent silence and avoid passing invalid input to `Math.Log10`. For positive amplitudes, it computes `20.0 * Math.Log10(amplitude)`, formats with `"F2"` and `CultureInfo.InvariantCulture`, then appends `" dBFS"`.


#### [[PipelineCommand.FormatDb]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string FormatDb(double amplitude)
```

**Called-by <-**
- [[PipelineCommand.CreateAudioTable]]


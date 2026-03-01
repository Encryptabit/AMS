---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# DspCommand::ExpandInputToken
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Expand an input token into either a predefined placeholder value or a filesystem path resolved against the base directory.**

This helper validates `token` with `string.IsNullOrWhiteSpace` and throws `InvalidOperationException` when it is empty. It then uses a switch expression to map `"{input}"`/`"{source}"` to `initialInput`, `"{prev}"`/`"{previous}"` to `previousOutput`, and delegates all other values to `ResolvePath(token, baseDirectory)`. The method is used by `ResolveInputs` to normalize both standard and MIDI input references.


#### [[DspCommand.ExpandInputToken]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ExpandInputToken(string token, string baseDirectory, string initialInput, string previousOutput)
```

**Calls ->**
- [[DspCommand.ResolvePath]]

**Called-by <-**
- [[DspCommand.ResolveInputs]]


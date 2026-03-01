---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# DspCommand::Sanitize
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Converts an arbitrary string into a filesystem-safe filename fragment by replacing invalid filename characters with underscores.**

`Sanitize` normalizes a candidate filename segment by iterating `Path.GetInvalidFileNameChars()` and repeatedly applying `value = value.Replace(c, '_')` for each invalid character. It performs no trimming, casing, or null checks, and simply returns the transformed string. In `ResolveNodeOutput`, this is used to convert node/plugin-derived stems into filesystem-safe output names.


#### [[DspCommand.Sanitize]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string Sanitize(string value)
```

**Called-by <-**
- [[DspCommand.ResolveNodeOutput]]


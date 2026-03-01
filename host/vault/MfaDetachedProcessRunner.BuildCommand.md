---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaDetachedProcessRunner.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# MfaDetachedProcessRunner::BuildCommand
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaDetachedProcessRunner.cs`

## Summary
**It creates the final MFA command-line string from a subcommand and optional argument payload.**

`BuildCommand` constructs the MFA CLI invocation by initializing a `StringBuilder` with `"mfa "`, appending `subcommand.Trim()`, and conditionally appending a space plus `args.Trim()` when `args` is non-empty/whitespace. It performs lightweight normalization (trimming) but no quoting or escaping, returning the raw assembled command string.


#### [[MfaDetachedProcessRunner.BuildCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string BuildCommand(string subcommand, string args)
```

**Called-by <-**
- [[MfaDetachedProcessRunner.RunAsync]]


---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# MfaProcessSupervisor::BuildCommand
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`

## Summary
**Compose a normalized `mfa` command line from a subcommand and optional argument text.**

`BuildCommand` constructs the final MFA CLI invocation using a `StringBuilder` initialized with `"mfa "`, then appends `subcommand.Trim()`. If `args` is non-empty/non-whitespace, it appends a separating space plus `args.Trim()`. The method performs only whitespace normalization and concatenation, returning the assembled command string without escaping or validation beyond whitespace checks.


#### [[MfaProcessSupervisor.BuildCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string BuildCommand(string subcommand, string args)
```

**Called-by <-**
- [[MfaProcessSupervisor.RunAsync]]


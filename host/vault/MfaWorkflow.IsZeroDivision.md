---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs"
access_modifier: "private"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# MfaWorkflow::IsZeroDivision
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs`

## Summary
**It detects whether an MFA command failed with a ZeroDivisionError by scanning stderr lines.**

`IsZeroDivision` is a predicate helper that inspects MFA stderr output and returns `true` when any line contains `"ZeroDivisionError"` using a case-insensitive comparison (`StringComparison.OrdinalIgnoreCase`). It performs no mutation and simply classifies a specific known failure pattern from `MfaCommandResult.StdErr`.


#### [[MfaWorkflow.IsZeroDivision]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool IsZeroDivision(MfaCommandResult result)
```


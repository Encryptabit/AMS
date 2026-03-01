---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaInvocationContext.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
---
# RestoreScope::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaInvocationContext.cs`

## Summary
**It records the previous async-local label so scope disposal can restore it.**

The `RestoreScope` constructor captures the prior invocation label by assigning the incoming `previous` value to the readonly `_previous` field. This stored state is later used by `Dispose()` to restore `MfaInvocationContext.CurrentLabel` after scope exit.


#### [[RestoreScope..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public RestoreScope(string previous)
```


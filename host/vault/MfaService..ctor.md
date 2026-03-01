---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaService.cs"
access_modifier: "internal"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/di
---
# MfaService::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaService.cs`

## Summary
**It initializes `MfaService` with process-mode and optional workspace-root settings used by later MFA command invocations.**

The constructor is a thin initializer that captures runtime execution configuration into readonly fields: `useDedicatedProcess` into `_useDedicatedProcess` and `workspaceRoot` into `_workspaceRoot`. It performs no branching or validation, deferring behavior to `RunCommandAsync`, which uses these fields to choose detached-process execution and pass the workspace root downstream.


#### [[MfaService..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal MfaService(bool useDedicatedProcess = false, string workspaceRoot = null)
```


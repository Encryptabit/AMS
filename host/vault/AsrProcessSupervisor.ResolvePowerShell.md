---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs"
access_modifier: "private"
complexity: 5
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AsrProcessSupervisor::ResolvePowerShell
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`

## Summary
**It resolves which PowerShell binary should be used to start managed ASR scripts.**

`ResolvePowerShell` chooses the PowerShell executable with environment and platform-aware fallback logic. It returns `AMS_ASR_POWERSHELL` when configured; otherwise on Windows it probes `PATH` via `TryFindOnPath` preferring `pwsh.exe`, then `powershell.exe`, and finally falls back to `"powershell"`. On non-Windows it defaults to `"pwsh"`.


#### [[AsrProcessSupervisor.ResolvePowerShell]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ResolvePowerShell()
```

**Calls ->**
- [[AsrProcessSupervisor.TryFindOnPath]]

**Called-by <-**
- [[AsrProcessSupervisor.BuildStartInfo]]
- [[AsrProcessSupervisor.CreateStartInfoForScript]]


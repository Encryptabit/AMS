---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs"
access_modifier: "private"
complexity: 11
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/factory
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AsrProcessSupervisor::CreateStartInfoForScript
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`

## Summary
**It converts a user-provided startup script path into an OS-appropriate process launch configuration for the ASR supervisor.**

`CreateStartInfoForScript` builds a supervised `ProcessStartInfo` for an explicit ASR startup script path after validating that the file exists; missing scripts are logged (`Log.Debug`) and return `null`. It dispatches by extension and OS: `.ps1` on Windows via `ResolvePowerShell` with `-NoProfile -ExecutionPolicy Bypass -File`, `.bat/.cmd` on Windows via `cmd.exe /c`, and `.py` cross-platform via configured `AMS_ASR_PYTHON` (fallback `python`/`python3`). All supported branches configure non-shell, redirected, no-window execution and set working directory to the script’s directory. Unsupported extensions are logged and return `null`.


#### [[AsrProcessSupervisor.CreateStartInfoForScript]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static ProcessStartInfo CreateStartInfoForScript(string scriptPath)
```

**Calls ->**
- [[AsrProcessSupervisor.ResolvePowerShell]]
- [[Log.Debug]]

**Called-by <-**
- [[AsrProcessSupervisor.BuildStartInfo]]


---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/factory
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AsrProcessSupervisor::BuildStartInfo
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`

## Summary
**It builds the process launch configuration for starting the ASR service from explicit overrides or repository defaults.**

`BuildStartInfo` resolves how to launch the ASR service by first honoring an explicit script path from `AMS_ASR_START_SCRIPT` via `CreateStartInfoForScript`. If unset, it locates the repo root (`ResolveRepoRoot`), validates `services/asr-nemo`, and then selects an OS-specific entrypoint: on Windows, `start_service.ps1` with PowerShell (`ResolvePowerShell` + bypass execution policy); on non-Windows, `app.py` with `AMS_ASR_PYTHON` or `python3`. For each valid path it returns a `ProcessStartInfo` configured for headless supervised execution (`UseShellExecute=false`, redirected stdout/stderr, `CreateNoWindow=true`, working directory set); otherwise it returns `null`.


#### [[AsrProcessSupervisor.BuildStartInfo]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static ProcessStartInfo BuildStartInfo()
```

**Calls ->**
- [[AsrProcessSupervisor.CreateStartInfoForScript]]
- [[AsrProcessSupervisor.ResolvePowerShell]]
- [[AsrProcessSupervisor.ResolveRepoRoot]]

**Called-by <-**
- [[AsrProcessSupervisor.StartManagedProcess]]


---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs"
access_modifier: "public"
base_class: ~
interfaces: []
member_count: 15
dependency_count: 0
pattern: ~
tags:
  - class
---

# AsrProcessSupervisor

> Class in `Ams.Core.Application.Processes`

**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`

## Properties
- `NemoEnabled`: bool
- `StatusLabel`: string
- `StatusDescription`: string
- `BaseUrl`: string?
- `Gate`: SemaphoreSlim
- `ShutdownLock`: object
- `_process`: Process?
- `_ownsProcess`: bool
- `_managedBaseUrl`: string?
- `_repoRoot`: string?
- `_warmupTask`: Task?
- `_state`: SupervisorState
- `StartupTimeout`: TimeSpan
- `HealthInterval`: TimeSpan
- `DisableAutoStartEnv`: string
- `StartScriptEnv`: string
- `PowerShellEnv`: string
- `PythonEnv`: string

## Members
- [[AsrProcessSupervisor.RegisterForShutdown]]
- [[AsrProcessSupervisor.TriggerBackgroundWarmup]]
- [[AsrProcessSupervisor.EnsureServiceReadyAsync]]
- [[AsrProcessSupervisor.Shutdown]]
- [[AsrProcessSupervisor.IsHealthyAsync]]
- [[AsrProcessSupervisor.WaitForHealthyAsync]]
- [[AsrProcessSupervisor.StartManagedProcess]]
- [[AsrProcessSupervisor.KillProcess]]
- [[AsrProcessSupervisor.BuildStartInfo]]
- [[AsrProcessSupervisor.CreateStartInfoForScript]]
- [[AsrProcessSupervisor.ResolvePowerShell]]
- [[AsrProcessSupervisor.TryFindOnPath]]
- [[AsrProcessSupervisor.ResolveRepoRoot]]
- [[AsrProcessSupervisor.IsLocalBaseUrl]]
- [[AsrProcessSupervisor.IsAutoStartDisabled]]


---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "public"
base_class: ~
interfaces: []
member_count: 19
dependency_count: 0
pattern: ~
tags:
  - class
---

# MfaProcessSupervisor

> Class in `Ams.Core.Application.Processes`

**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`

## Properties
- `ReadyToken`: string
- `ExitToken`: string
- `QuitToken`: string
- `CommandGate`: SemaphoreSlim
- `StartLock`: object
- `JsonOptions`: JsonSerializerOptions
- `_process`: Process?
- `_stdin`: StreamWriter?
- `_lineChannel`: Channel<ProcessLine>?
- `_stdoutPump`: Task?
- `_stderrPump`: Task?
- `_pumpCts`: CancellationTokenSource?
- `_isReady`: bool
- `_scriptPath`: string?
- `_activePumpCount`: int
- `_startTask`: Task?

## Members
- [[MfaProcessSupervisor.RegisterForShutdown]]
- [[MfaProcessSupervisor.TriggerBackgroundWarmup]]
- [[MfaProcessSupervisor.EnsureReadyAsync]]
- [[MfaProcessSupervisor.RunAsync]]
- [[MfaProcessSupervisor.EnsureStartedAsync]]
- [[MfaProcessSupervisor.StartProcessAsync]]
- [[MfaProcessSupervisor.EnsureBootstrapScript]]
- [[MfaProcessSupervisor.WaitForCommandCompletionAsync]]
- [[MfaProcessSupervisor.WaitForReadyAsync]]
- [[MfaProcessSupervisor.PumpAsync]]
- [[MfaProcessSupervisor.Shutdown]]
- [[MfaProcessSupervisor.TearDownProcess]]
- [[MfaProcessSupervisor.IsProcessRunning]]
- [[MfaProcessSupervisor.BuildCommand]]
- [[MfaProcessSupervisor.NormalizeWorkingDirectory]]
- [[MfaProcessSupervisor.ResolveWorkspaceRootForSupervisor]]
- [[MfaProcessSupervisor.ResolvePwshExecutable]]
- [[MfaProcessSupervisor.ResolveBootstrapSequence]]
- [[MfaProcessSupervisor.BuildSupervisorScript]]


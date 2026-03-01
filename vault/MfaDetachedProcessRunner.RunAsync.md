---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaDetachedProcessRunner.cs"
access_modifier: "public"
complexity: 5
fan_in: 1
fan_out: 8
tags:
  - method
---
# MfaDetachedProcessRunner::RunAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaDetachedProcessRunner.cs`


#### [[MfaDetachedProcessRunner.RunAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Task<MfaCommandResult> RunAsync(string subcommand, string args, string workingDirectory, string workspaceRoot, CancellationToken cancellationToken)
```

**Calls ->**
- [[MfaDetachedProcessRunner.BuildCommand]]
- [[MfaDetachedProcessRunner.NormalizeWorkingDirectory]]
- [[MfaDetachedProcessRunner.PumpStreamAsync]]
- [[MfaDetachedProcessRunner.TryDeleteScript]]
- [[MfaDetachedProcessRunner.WriteScript]]
- [[MfaWorkspaceResolver.ResolvePreferredRoot]]
- [[MfaProcessSupervisor.ResolvePwshExecutable]]
- [[Log.Debug]]

**Called-by <-**
- [[MfaService.RunCommandAsync]]


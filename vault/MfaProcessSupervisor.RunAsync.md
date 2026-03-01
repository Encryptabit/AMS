---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "internal"
complexity: 2
fan_in: 1
fan_out: 5
tags:
  - method
---
# MfaProcessSupervisor::RunAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`


#### [[MfaProcessSupervisor.RunAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static Task<MfaCommandResult> RunAsync(string subcommand, string args, string workingDirectory, CancellationToken cancellationToken)
```

**Calls ->**
- [[MfaProcessSupervisor.BuildCommand]]
- [[MfaProcessSupervisor.EnsureStartedAsync]]
- [[MfaProcessSupervisor.NormalizeWorkingDirectory]]
- [[MfaProcessSupervisor.WaitForCommandCompletionAsync]]
- [[Log.Debug]]

**Called-by <-**
- [[MfaService.RunCommandAsync]]


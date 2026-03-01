---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaService.cs"
access_modifier: "private"
complexity: 2
fan_in: 4
fan_out: 2
tags:
  - method
---
# MfaService::RunCommandAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaService.cs`


#### [[MfaService.RunCommandAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<MfaCommandResult> RunCommandAsync(string subcommand, string args, string workingDirectory, CancellationToken cancellationToken)
```

**Calls ->**
- [[MfaDetachedProcessRunner.RunAsync]]
- [[MfaProcessSupervisor.RunAsync]]

**Called-by <-**
- [[MfaService.AddWordsAsync]]
- [[MfaService.AlignAsync]]
- [[MfaService.GeneratePronunciationsAsync]]
- [[MfaService.ValidateAsync]]


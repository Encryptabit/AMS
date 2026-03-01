---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaDetachedProcessRunner.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
---
# MfaDetachedProcessRunner::BuildScript
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaDetachedProcessRunner.cs`


#### [[MfaDetachedProcessRunner.BuildScript]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string BuildScript(string command, string workingDirectory)
```

**Calls ->**
- [[MfaProcessSupervisor.ResolveBootstrapSequence]]

**Called-by <-**
- [[MfaDetachedProcessRunner.WriteScript]]


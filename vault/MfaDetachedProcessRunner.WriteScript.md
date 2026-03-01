---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaDetachedProcessRunner.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
---
# MfaDetachedProcessRunner::WriteScript
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaDetachedProcessRunner.cs`


#### [[MfaDetachedProcessRunner.WriteScript]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string WriteScript(string command, string workingDirectory)
```

**Calls ->**
- [[MfaDetachedProcessRunner.BuildScript]]

**Called-by <-**
- [[MfaDetachedProcessRunner.RunAsync]]


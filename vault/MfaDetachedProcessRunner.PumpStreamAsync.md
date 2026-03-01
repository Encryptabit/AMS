---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaDetachedProcessRunner.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
---
# MfaDetachedProcessRunner::PumpStreamAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaDetachedProcessRunner.cs`


#### [[MfaDetachedProcessRunner.PumpStreamAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task PumpStreamAsync(StreamReader reader, List<string> sink)
```

**Called-by <-**
- [[MfaDetachedProcessRunner.RunAsync]]


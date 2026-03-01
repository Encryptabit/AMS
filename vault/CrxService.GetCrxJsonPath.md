---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/CrxService.cs"
access_modifier: "private"
complexity: 2
fan_in: 3
fan_out: 0
tags:
  - method
---
# CrxService::GetCrxJsonPath
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/CrxService.cs`


#### [[CrxService.GetCrxJsonPath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private string GetCrxJsonPath(bool createDir = true)
```

**Called-by <-**
- [[CrxService.TryReadJsonEntries]]
- [[CrxService.TryRemoveJsonEntry]]
- [[CrxService.WriteJsonEntries]]


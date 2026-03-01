---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/CrxService.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 4
tags:
  - method
---
# CrxService::EnsureJsonSeededFromExcel
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/CrxService.cs`


#### [[CrxService.EnsureJsonSeededFromExcel]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void EnsureJsonSeededFromExcel()
```

**Calls ->**
- [[CrxService.BuildSeededLegacyEntry]]
- [[CrxService.TryReadExcelEntries]]
- [[CrxService.TryReadJsonEntries]]
- [[CrxService.WriteJsonEntries]]

**Called-by <-**
- [[CrxService.GetEntries]]


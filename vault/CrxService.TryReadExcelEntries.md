---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/CrxService.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 2
tags:
  - method
---
# CrxService::TryReadExcelEntries
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/CrxService.cs`


#### [[CrxService.TryReadExcelEntries]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private IReadOnlyList<CrxEntry> TryReadExcelEntries()
```

**Calls ->**
- [[CrxService.GetCrxExcelPath]]
- [[CrxService.TryReadExcelEntriesOpenXml]]

**Called-by <-**
- [[CrxService.EnsureJsonSeededFromExcel]]


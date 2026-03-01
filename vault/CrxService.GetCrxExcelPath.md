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
# CrxService::GetCrxExcelPath
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/CrxService.cs`


#### [[CrxService.GetCrxExcelPath]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.CrxService.GetCrxExcelPath(System.Boolean)">
    <summary>
    Get the path to the CRX Excel file for the current book.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private string GetCrxExcelPath(bool createDir = true)
```

**Called-by <-**
- [[CrxService.AppendCrxEntry]]
- [[CrxService.EnsureExcelReady]]
- [[CrxService.TryReadExcelEntries]]


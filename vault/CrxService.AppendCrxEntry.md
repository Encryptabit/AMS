---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/CrxService.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
---
# CrxService::AppendCrxEntry
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/CrxService.cs`


#### [[CrxService.AppendCrxEntry]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.CrxService.AppendCrxEntry(Ams.Workstation.Server.Models.CrxEntry)">
    <summary>
    Append a CRX entry to the Excel workbook.
    On first use, copies the BASE_CRX.xlsx template to preserve formatting.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void AppendCrxEntry(CrxEntry entry)
```

**Calls ->**
- [[CrxService.GetCrxExcelPath]]

**Called-by <-**
- [[CrxService.Submit]]


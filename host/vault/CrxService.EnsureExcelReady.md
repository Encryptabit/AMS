---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/CrxService.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
---
# CrxService::EnsureExcelReady
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/CrxService.cs`


#### [[CrxService.EnsureExcelReady]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.CrxService.EnsureExcelReady">
    <summary>
    Validate that the Excel workbook is ready (template exists or file already created).
    Called before audio export to fail fast and avoid orphan WAV files.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void EnsureExcelReady()
```

**Calls ->**
- [[CrxService.GetCrxExcelPath]]

**Called-by <-**
- [[CrxService.Submit]]


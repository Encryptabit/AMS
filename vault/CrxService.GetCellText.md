---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/CrxService.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 0
tags:
  - method
---
# CrxService::GetCellText
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/CrxService.cs`


#### [[CrxService.GetCellText]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string GetCellText(XElement cell, IReadOnlyList<string> shared, XNamespace ns)
```

**Called-by <-**
- [[CrxService.TryReadExcelEntriesOpenXml]]


---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/CrxService.cs"
access_modifier: "private"
complexity: 11
fan_in: 1
fan_out: 3
tags:
  - method
---
# CrxService::TryReadExcelEntriesOpenXml
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/CrxService.cs`


#### [[CrxService.TryReadExcelEntriesOpenXml]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyList<CrxEntry> TryReadExcelEntriesOpenXml(string path)
```

**Calls ->**
- [[CrxService.ExtractColumnIndex]]
- [[CrxService.GetCellText]]
- [[CrxService.ReadSharedStrings]]

**Called-by <-**
- [[CrxService.TryReadExcelEntries]]


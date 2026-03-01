---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/CrxService.cs"
access_modifier: "private"
complexity: 2
fan_in: 3
fan_out: 1
tags:
  - method
---
# CrxService::WriteJsonEntries
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/CrxService.cs`


#### [[CrxService.WriteJsonEntries]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void WriteJsonEntries(IReadOnlyList<CrxEntry> entries)
```

**Calls ->**
- [[CrxService.GetCrxJsonPath]]

**Called-by <-**
- [[CrxService.AppendOrUpdateJsonEntry]]
- [[CrxService.EnsureJsonSeededFromExcel]]
- [[CrxService.TryRemoveJsonEntry]]


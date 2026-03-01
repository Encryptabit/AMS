---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/CrxService.cs"
access_modifier: "private"
complexity: 4
fan_in: 4
fan_out: 1
tags:
  - method
---
# CrxService::TryReadJsonEntries
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/CrxService.cs`


#### [[CrxService.TryReadJsonEntries]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private IReadOnlyList<CrxEntry> TryReadJsonEntries()
```

**Calls ->**
- [[CrxService.GetCrxJsonPath]]

**Called-by <-**
- [[CrxService.AppendOrUpdateJsonEntry]]
- [[CrxService.EnsureJsonSeededFromExcel]]
- [[CrxService.GetEntries]]
- [[CrxService.TryRemoveJsonEntry]]


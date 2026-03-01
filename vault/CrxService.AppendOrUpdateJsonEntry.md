---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/CrxService.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 2
tags:
  - method
---
# CrxService::AppendOrUpdateJsonEntry
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/CrxService.cs`


#### [[CrxService.AppendOrUpdateJsonEntry]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void AppendOrUpdateJsonEntry(CrxEntry entry)
```

**Calls ->**
- [[CrxService.TryReadJsonEntries]]
- [[CrxService.WriteJsonEntries]]

**Called-by <-**
- [[CrxService.Submit]]


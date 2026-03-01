---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/CrxService.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 3
tags:
  - method
---
# CrxService::TryRemoveJsonEntry
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/CrxService.cs`


#### [[CrxService.TryRemoveJsonEntry]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void TryRemoveJsonEntry(int errorNumber)
```

**Calls ->**
- [[CrxService.GetCrxJsonPath]]
- [[CrxService.TryReadJsonEntries]]
- [[CrxService.WriteJsonEntries]]

**Called-by <-**
- [[CrxService.Submit]]


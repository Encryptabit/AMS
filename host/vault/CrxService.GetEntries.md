---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/CrxService.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 2
tags:
  - method
---
# CrxService::GetEntries
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/CrxService.cs`


#### [[CrxService.GetEntries]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.CrxService.GetEntries">
    <summary>
    Read all CRX entries from the CRX JSON artifact.
    If JSON is missing but legacy Excel entries exist, seed JSON once from Excel.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IReadOnlyList<CrxEntry> GetEntries()
```

**Calls ->**
- [[CrxService.EnsureJsonSeededFromExcel]]
- [[CrxService.TryReadJsonEntries]]


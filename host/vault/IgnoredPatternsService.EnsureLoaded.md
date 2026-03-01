---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/IgnoredPatternsService.cs"
access_modifier: "private"
complexity: 2
fan_in: 5
fan_out: 2
tags:
  - method
---
# IgnoredPatternsService::EnsureLoaded
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/IgnoredPatternsService.cs`


#### [[IgnoredPatternsService.EnsureLoaded]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void EnsureLoaded()
```

**Calls ->**
- [[IgnoredPatternsService.GetCurrentBookId]]
- [[IgnoredPatternsService.Load]]

**Called-by <-**
- [[IgnoredPatternsService.GetIgnoredKeys]]
- [[IgnoredPatternsService.IsIgnored]]
- [[IgnoredPatternsService.ResetCurrentBook]]
- [[IgnoredPatternsService.SetIgnored]]
- [[IgnoredPatternsService.ToggleIgnored]]


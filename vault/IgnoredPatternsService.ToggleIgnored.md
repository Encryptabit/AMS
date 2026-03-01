---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/IgnoredPatternsService.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 2
tags:
  - method
---
# IgnoredPatternsService::ToggleIgnored
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/IgnoredPatternsService.cs`


#### [[IgnoredPatternsService.ToggleIgnored]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool ToggleIgnored(string key)
```

**Calls ->**
- [[IgnoredPatternsService.EnsureLoaded]]
- [[IgnoredPatternsService.Save]]


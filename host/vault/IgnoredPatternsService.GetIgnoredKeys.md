---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/IgnoredPatternsService.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
---
# IgnoredPatternsService::GetIgnoredKeys
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/IgnoredPatternsService.cs`


#### [[IgnoredPatternsService.GetIgnoredKeys]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IReadOnlySet<string> GetIgnoredKeys()
```

**Calls ->**
- [[IgnoredPatternsService.EnsureLoaded]]

**Called-by <-**
- [[BookMetadataResetService.ResetCurrentBook]]


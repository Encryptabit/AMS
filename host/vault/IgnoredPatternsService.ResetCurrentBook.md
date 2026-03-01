---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/IgnoredPatternsService.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 2
tags:
  - method
---
# IgnoredPatternsService::ResetCurrentBook
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/IgnoredPatternsService.cs`


#### [[IgnoredPatternsService.ResetCurrentBook]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void ResetCurrentBook()
```

**Calls ->**
- [[IgnoredPatternsService.EnsureLoaded]]
- [[IgnoredPatternsService.Save]]

**Called-by <-**
- [[BookMetadataResetService.ResetCurrentBook]]


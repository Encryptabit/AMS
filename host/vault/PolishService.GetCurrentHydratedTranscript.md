---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/PolishService.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
---
# PolishService::GetCurrentHydratedTranscript
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/PolishService.cs`


#### [[PolishService.GetCurrentHydratedTranscript]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.PolishService.GetCurrentHydratedTranscript">
    <summary>
    Loads the hydrated transcript for the current chapter, if available.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private HydratedTranscript GetCurrentHydratedTranscript()
```

**Calls ->**
- [[BlazorWorkspace.TryGetHydratedTranscript]]

**Called-by <-**
- [[PolishService.StageReplacement]]


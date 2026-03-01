---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/PolishService.cs"
access_modifier: "private"
complexity: 3
fan_in: 2
fan_out: 1
tags:
  - method
---
# PolishService::TrimPickupForReplacement
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/PolishService.cs`


#### [[PolishService.TrimPickupForReplacement]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static AudioBuffer TrimPickupForReplacement(StagedReplacement item, AudioBuffer pickupBuffer)
```

**Calls ->**
- [[AudioProcessor.Trim]]

**Called-by <-**
- [[PolishService.ApplyReplacementAsync]]
- [[PolishService.GeneratePreview]]


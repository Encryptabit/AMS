---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/PolishService.cs"
access_modifier: "private"
complexity: 21
fan_in: 1
fan_out: 0
tags:
  - method
  - danger/high-complexity
---
# PolishService::ResolveSourceBitDepthOrThrow
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/PolishService.cs`

> [!danger] High Complexity (21)
> Cyclomatic complexity: 21. Consider refactoring into smaller methods.


#### [[PolishService.ResolveSourceBitDepthOrThrow]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static int ResolveSourceBitDepthOrThrow(AudioBuffer buffer)
```

**Called-by <-**
- [[PolishService.PersistCorrectedBuffer]]


---
namespace: "Ams.Core.Pipeline"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Pipeline/SentenceRefinementService.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
---
# SentenceRefinementService::RunAeneasAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Pipeline/SentenceRefinementService.cs`


#### [[SentenceRefinementService.RunAeneasAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<List<(double begin, double end)>> RunAeneasAsync(string audioPath, List<string> lines, string language)
```

**Calls ->**
- [[ParseDouble]]

**Called-by <-**
- [[SentenceRefinementService.RefineAsync]]


---
namespace: "Ams.Core.Pipeline"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Pipeline/SentenceRefinementService.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
---
# SentenceRefinementService::DetectSilencesAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Pipeline/SentenceRefinementService.cs`


#### [[SentenceRefinementService.DetectSilencesAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<SilenceInfo[]> DetectSilencesAsync(string audioPath, double thresholdDb, double minDurationSec)
```

**Calls ->**
- [[SentenceRefinementService.ParseSilenceOutput]]

**Called-by <-**
- [[SentenceRefinementService.RefineAsync]]


---
namespace: "Ams.Core.Services"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/PipelineService.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
---
# PipelineService::CountMissingPhonemes
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/PipelineService.cs`


#### [[PipelineService.CountMissingPhonemes]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static int CountMissingPhonemes(BookIndex index)
```

**Calls ->**
- [[PronunciationHelper.NormalizeForLookup]]

**Called-by <-**
- [[PipelineService.EnsurePhonemesAsync]]


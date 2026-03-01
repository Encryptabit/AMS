---
namespace: "Ams.Core.Validation"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Validation/ScriptValidator.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 2
tags:
  - method
---
# ScriptValidator::GenerateSegmentStats
**Path**: `home/cari/repos/AMS/host/Ams.Core/Validation/ScriptValidator.cs`


#### [[ScriptValidator.GenerateSegmentStats]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private List<SegmentStats> GenerateSegmentStats(AsrResponse asrResponse, string scriptText)
```

**Calls ->**
- [[TextNormalizer.Normalize]]
- [[ScriptValidator.CalculateSegmentWER]]

**Called-by <-**
- [[ScriptValidator.Validate]]


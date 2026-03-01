---
namespace: "Ams.Core.Validation"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Validation/ScriptValidator.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 0
tags:
  - method
---
# ScriptValidator::CalculateWordErrorStats
**Path**: `home/cari/repos/AMS/host/Ams.Core/Validation/ScriptValidator.cs`


#### [[ScriptValidator.CalculateWordErrorStats]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private (int correct, int substitutions, int insertions, int deletions) CalculateWordErrorStats(List<ScriptValidator.AlignmentResult> alignment)
```

**Called-by <-**
- [[ScriptValidator.Validate]]


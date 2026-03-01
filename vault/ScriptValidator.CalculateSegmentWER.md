---
namespace: "Ams.Core.Validation"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Validation/ScriptValidator.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 2
tags:
  - method
---
# ScriptValidator::CalculateSegmentWER
**Path**: `home/cari/repos/AMS/host/Ams.Core/Validation/ScriptValidator.cs`


#### [[ScriptValidator.CalculateSegmentWER]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private double CalculateSegmentWER(string expected, string actual)
```

**Calls ->**
- [[TextNormalizer.TokenizeWords]]
- [[ScriptValidator.CalculateEditDistance]]

**Called-by <-**
- [[ScriptValidator.GenerateSegmentStats]]


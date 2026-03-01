---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 0
tags:
  - method
---
# MfaPronunciationProvider::ExpandCombinations
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs`


#### [[MfaPronunciationProvider.ExpandCombinations]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<string> ExpandCombinations(List<string> basePronunciations, List<string> variants, int maxCount)
```

**Called-by <-**
- [[MfaPronunciationProvider.ComposeLexemePronunciations]]


---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
---
# MfaPronunciationProvider::ComposeLexemePronunciations
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs`


#### [[MfaPronunciationProvider.ComposeLexemePronunciations]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyDictionary<string, string[]> ComposeLexemePronunciations(IReadOnlyDictionary<string, IReadOnlyList<string>> lexemeComponents, IReadOnlyDictionary<string, List<string>> wordPronunciations)
```

**Calls ->**
- [[MfaPronunciationProvider.ExpandCombinations]]

**Called-by <-**
- [[MfaPronunciationProvider.GetPronunciationsAsync]]


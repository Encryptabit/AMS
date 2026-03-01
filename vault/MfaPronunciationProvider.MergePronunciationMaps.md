---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
---
# MfaPronunciationProvider::MergePronunciationMaps
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs`


#### [[MfaPronunciationProvider.MergePronunciationMaps]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyDictionary<string, string[]> MergePronunciationMaps(IReadOnlyDictionary<string, string[]> first, IReadOnlyDictionary<string, string[]> second)
```

**Called-by <-**
- [[MfaPronunciationProvider.GetPronunciationsAsync]]


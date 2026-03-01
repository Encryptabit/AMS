---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
---
# MfaPronunciationProvider::NormalizeVariantKey
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs`


#### [[MfaPronunciationProvider.NormalizeVariantKey]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string NormalizeVariantKey(string raw)
```

**Calls ->**
- [[PronunciationHelper.NormalizeForLookup]]

**Called-by <-**
- [[MfaPronunciationProvider.GetPronunciationsAsync]]


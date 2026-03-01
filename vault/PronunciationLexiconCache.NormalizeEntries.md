---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
---
# PronunciationLexiconCache::NormalizeEntries
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs`


#### [[PronunciationLexiconCache.NormalizeEntries]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Dictionary<string, string[]> NormalizeEntries(IReadOnlyDictionary<string, string[]> entries)
```

**Calls ->**
- [[PronunciationLexiconCache.NormalizeVariants]]

**Called-by <-**
- [[PronunciationLexiconCache.ReadCoreAsync]]


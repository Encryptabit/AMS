---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs"
access_modifier: "private"
complexity: 1
fan_in: 2
fan_out: 1
tags:
  - method
---
# PronunciationLexiconCache::NormalizeVariants
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs`


#### [[PronunciationLexiconCache.NormalizeVariants]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string[] NormalizeVariants(IEnumerable<string> variants)
```

**Calls ->**
- [[PronunciationLexiconCache.AppendVariants]]

**Called-by <-**
- [[PronunciationLexiconCache.MergeAsync]]
- [[PronunciationLexiconCache.NormalizeEntries]]


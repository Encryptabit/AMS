---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs"
access_modifier: "private"
complexity: 5
fan_in: 2
fan_out: 0
tags:
  - method
---
# PronunciationLexiconCache::AppendVariants
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs`


#### [[PronunciationLexiconCache.AppendVariants]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void AppendVariants(HashSet<string> set, IEnumerable<string> variants)
```

**Called-by <-**
- [[PronunciationLexiconCache.MergeVariants]]
- [[PronunciationLexiconCache.NormalizeVariants]]


---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs"
access_modifier: "private"
complexity: 8
fan_in: 2
fan_out: 3
tags:
  - method
---
# PronunciationLexiconCache::ReadCoreAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs`


#### [[PronunciationLexiconCache.ReadCoreAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<PronunciationLexiconCache.PronunciationLexiconCacheDocument> ReadCoreAsync(CancellationToken cancellationToken)
```

**Calls ->**
- [[PronunciationLexiconCache.EmptyDocument]]
- [[PronunciationLexiconCache.NormalizeEntries]]
- [[Log.Debug]]

**Called-by <-**
- [[PronunciationLexiconCache.MergeAsync]]
- [[PronunciationLexiconCache.ReadAsync]]


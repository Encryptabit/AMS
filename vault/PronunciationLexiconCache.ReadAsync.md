---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
---
# PronunciationLexiconCache::ReadAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs`


#### [[PronunciationLexiconCache.ReadAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<PronunciationLexiconCache.PronunciationLexiconCacheDocument> ReadAsync(CancellationToken cancellationToken)
```

**Calls ->**
- [[PronunciationLexiconCache.ReadCoreAsync]]

**Called-by <-**
- [[PronunciationLexiconCache.GetManyAsync]]


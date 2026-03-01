---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs"
access_modifier: "public"
complexity: 6
fan_in: 3
fan_out: 1
tags:
  - method
---
# PronunciationLexiconCache::GetManyAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs`


#### [[PronunciationLexiconCache.GetManyAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<IReadOnlyDictionary<string, string[]>> GetManyAsync(IReadOnlyCollection<string> lexemes, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[PronunciationLexiconCache.ReadAsync]]

**Called-by <-**
- [[MfaPronunciationProvider.GetPronunciationsAsync]]
- [[PronunciationLexiconCacheTests.Cache_IsModelScoped]]
- [[PronunciationLexiconCacheTests.Cache_PersistsAndReturnsHitsAcrossInstances]]


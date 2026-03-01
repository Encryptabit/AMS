---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs"
access_modifier: "public"
complexity: 7
fan_in: 3
fan_out: 5
tags:
  - method
---
# PronunciationLexiconCache::MergeAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs`


#### [[PronunciationLexiconCache.MergeAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<int> MergeAsync(IReadOnlyDictionary<string, string[]> updates, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[PronunciationLexiconCache.AreSame]]
- [[PronunciationLexiconCache.MergeVariants]]
- [[PronunciationLexiconCache.NormalizeVariants]]
- [[PronunciationLexiconCache.ReadCoreAsync]]
- [[PronunciationLexiconCache.WriteCoreAsync]]

**Called-by <-**
- [[MfaPronunciationProvider.GetPronunciationsAsync]]
- [[PronunciationLexiconCacheTests.Cache_IsModelScoped]]
- [[PronunciationLexiconCacheTests.Cache_PersistsAndReturnsHitsAcrossInstances]]


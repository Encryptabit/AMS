---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs"
access_modifier: "public"
complexity: 7
fan_in: 3
fan_out: 5
tags:
  - method
  - llm/async
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# PronunciationLexiconCache::MergeAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs`

## Summary
**It merges pronunciation updates into the persistent lexicon cache and returns the number of entries that actually changed.**

`MergeAsync` performs a serialized read-modify-write merge of pronunciation entries using the class-level `SemaphoreSlim` gate. It short-circuits on empty `updates`, then loads existing cache data via `ReadCoreAsync`, normalizes incoming variants (`NormalizeVariants`), skips invalid keys/empty variant sets, and merges per lexeme with `MergeVariants`, counting only effective changes via `AreSame`. When at least one entry changed, it persists a new cache document through `WriteCoreAsync`; otherwise it returns `0` without writing.


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


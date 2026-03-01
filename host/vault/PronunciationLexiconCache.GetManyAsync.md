---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs"
access_modifier: "public"
complexity: 6
fan_in: 3
fan_out: 1
tags:
  - method
  - llm/async
  - llm/data-access
  - llm/utility
  - llm/validation
---
# PronunciationLexiconCache::GetManyAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs`

## Summary
**It returns cached pronunciation variants for a requested lexeme set, filtering out misses and empty entries.**

`GetManyAsync` performs a batched cache lookup for pronunciations keyed by lexeme. It short-circuits to an empty case-insensitive dictionary when `lexemes.Count == 0`, then reads the persisted cache document via `ReadAsync`; if no entries exist it returns empty again. Otherwise it iterates requested lexemes and copies only hits with non-empty variant arrays (`variants is { Length: > 0 }`) into a new case-insensitive result map.


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


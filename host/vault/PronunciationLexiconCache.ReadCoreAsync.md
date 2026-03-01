---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs"
access_modifier: "private"
complexity: 8
fan_in: 2
fan_out: 3
tags:
  - method
  - llm/async
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# PronunciationLexiconCache::ReadCoreAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs`

## Summary
**It reads and validates the persisted pronunciation cache document, falling back to an empty cache state on invalid or unreadable data.**

`ReadCoreAsync` loads the model-scoped cache JSON from `_cacheFilePath` and defensively returns `EmptyDocument()` when the file is missing, empty, schema-mismatched, model-mismatched, or deserialization fails. On successful deserialize, it normalizes the entry map (`document.Entries ?? new Dictionary...`) through `NormalizeEntries` and returns a reconstructed `PronunciationLexiconCacheDocument` preserving schema/model/timestamp metadata. Non-cancellation exceptions are caught, logged with `Log.Debug`, and treated as cache misses by returning an empty document.


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


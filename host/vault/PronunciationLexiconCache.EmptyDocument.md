---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/factory
---
# PronunciationLexiconCache::EmptyDocument
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs`

## Summary
**It creates an initialized empty pronunciation cache document for the active model scope.**

`EmptyDocument` returns a freshly constructed `PronunciationLexiconCacheDocument` seeded with the current `SchemaVersion`, instance `_g2pModel`, current UTC timestamp, and an empty `Dictionary<string, string[]>` using `StringComparer.OrdinalIgnoreCase`. It is the canonical fallback document used when persisted cache data is unavailable or invalid.


#### [[PronunciationLexiconCache.EmptyDocument]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private PronunciationLexiconCache.PronunciationLexiconCacheDocument EmptyDocument()
```

**Called-by <-**
- [[PronunciationLexiconCache.ReadCoreAsync]]


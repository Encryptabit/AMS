---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/factory
  - llm/validation
---
# PronunciationLexiconCache::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs`

## Summary
**It initializes the pronunciation cache with an effective G2P model key and its resolved cache file path.**

The constructor normalizes model identity and cache location for the lexicon cache instance. It sets `_g2pModel` to the provided `g2pModel` when non-empty (trimmed), otherwise falls back to `MfaService.DefaultG2pModel`, and then computes `_cacheFilePath` by calling `ResolveCacheFilePath(_g2pModel)`. This binds cache reads/writes to a model-specific backing file.


#### [[PronunciationLexiconCache..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public PronunciationLexiconCache(string g2pModel)
```

**Calls ->**
- [[PronunciationLexiconCache.ResolveCacheFilePath]]


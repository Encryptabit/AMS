---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
---
# PronunciationLexiconCache::ResolveCacheFilePath
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs`

## Summary
**It resolves the model-scoped pronunciation cache file location from explicit env overrides or local app-data defaults.**

`ResolveCacheFilePath` determines the cache JSON path using environment-driven precedence. It first honors `AMS_PHONEME_CACHE_FILE` (trimmed and normalized with `Path.GetFullPath`); otherwise it chooses a base directory from `AMS_PHONEME_CACHE_DIR`, falling back to `<LocalApplicationData>/AMS/PronunciationCache`. It then sanitizes the model key via `SanitizeFileName(g2pModel)` and returns `<baseDirectory>/<safeModel>.json`.


#### [[PronunciationLexiconCache.ResolveCacheFilePath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ResolveCacheFilePath(string g2pModel)
```

**Calls ->**
- [[PronunciationLexiconCache.SanitizeFileName]]

**Called-by <-**
- [[PronunciationLexiconCache..ctor]]


---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/async
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# PronunciationLexiconCache::WriteCoreAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs`

## Summary
**It writes the pronunciation cache document to disk using a temp-file swap strategy for safer updates.**

`WriteCoreAsync` persists the cache document atomically by serializing JSON to a unique temp file (`_cacheFilePath + ".tmp-" + Guid...`) and then replacing the target via `File.Move(..., overwrite: true)`. It validates the destination path by extracting `Path.GetDirectoryName(_cacheFilePath)` and throws `InvalidOperationException` when invalid, otherwise ensures the directory exists with `Directory.CreateDirectory`. A `finally` block performs best-effort cleanup of leftover temp files.


#### [[PronunciationLexiconCache.WriteCoreAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task WriteCoreAsync(PronunciationLexiconCache.PronunciationLexiconCacheDocument document, CancellationToken cancellationToken)
```

**Called-by <-**
- [[PronunciationLexiconCache.MergeAsync]]


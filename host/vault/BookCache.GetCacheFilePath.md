---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookCache.cs"
access_modifier: "private"
complexity: 2
fan_in: 3
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/factory
  - llm/data-access
  - llm/validation
---
# BookCache::GetCacheFilePath
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookCache.cs`

## Summary
**Generates a stable, safe cache file path for a source file using sanitized name + hashed suffix.**

`GetCacheFilePath` deterministically maps a source-file path to a cache JSON filename under `_cacheDirectory`. It computes a SHA-256 hash of the full source path (`ComputeStringHash`), extracts the source stem (`Path.GetFileNameWithoutExtension`), strips invalid filename characters, and truncates the sanitized stem to 50 chars. It then builds `<safeName>_<first8Hash>.json` and returns `Path.Combine(_cacheDirectory, cacheFileName)`. This yields filesystem-safe, collision-resistant cache keys with readable prefixes.


#### [[BookCache.GetCacheFilePath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private string GetCacheFilePath(string sourceFile)
```

**Calls ->**
- [[BookCache.ComputeStringHash]]

**Called-by <-**
- [[BookCache.GetAsync]]
- [[BookCache.RemoveAsync]]
- [[BookCache.SetAsync]]


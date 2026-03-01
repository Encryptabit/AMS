---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookCache.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/data-access
---
# BookCache::GetDefaultCacheDirectory
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookCache.cs`

## Summary
**Builds the default filesystem path used for storing book cache files.**

`GetDefaultCacheDirectory` derives the cache root from the user-local application data location and appends fixed subfolders. It calls `Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)` and returns `Path.Combine(baseDir, "AMS", "BookCache")`. The method performs deterministic path composition only and does not create directories itself.


#### [[BookCache.GetDefaultCacheDirectory]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string GetDefaultCacheDirectory()
```

**Called-by <-**
- [[BookCache..ctor]]


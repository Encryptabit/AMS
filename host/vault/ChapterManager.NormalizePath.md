---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# ChapterManager::NormalizePath
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

## Summary
**Normalizes a filesystem path into a canonical absolute form without trailing separators.**

`NormalizePath` canonicalizes a path for reliable comparisons by converting it to an absolute path (`Path.GetFullPath(path)`) and trimming trailing directory separators (`Path.DirectorySeparatorChar`, `Path.AltDirectorySeparatorChar`). It performs no existence checks and returns a normalized string suitable for case-insensitive equality matching.


#### [[ChapterManager.NormalizePath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string NormalizePath(string path)
```

**Called-by <-**
- [[ChapterManager.TryMatchByRootPath]]


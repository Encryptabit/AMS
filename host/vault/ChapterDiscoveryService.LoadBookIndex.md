---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDiscoveryService.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/error-handling
  - llm/utility
---
# ChapterDiscoveryService::LoadBookIndex
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDiscoveryService.cs`

## Summary
**Tries to load a book index from disk, returning `null` when the file is missing or unreadable.**

`LoadBookIndex` attempts best-effort loading of `book-index.json` from the given root directory. It builds `indexPath`, returns `null` if the file is absent, otherwise reads the JSON text and deserializes `BookIndex` using local `JsonOptions` (`PropertyNameCaseInsensitive = true`). Any read/deserialize exception is swallowed and treated as `null`, allowing chapter discovery to proceed without index metadata.


#### [[ChapterDiscoveryService.LoadBookIndex]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static BookIndex LoadBookIndex(string rootPath)
```

**Called-by <-**
- [[ChapterDiscoveryService.DiscoverChapters]]


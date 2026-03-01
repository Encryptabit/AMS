---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDiscoveryService.cs"
access_modifier: "private"
complexity: 14
fan_in: 2
fan_out: 2
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/factory
  - llm/validation
  - llm/error-handling
---
# ChapterDiscoveryService::DiscoverChaptersCore
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDiscoveryService.cs`

## Summary
**Discovers chapter WAV files, matches them to book sections when possible, and returns a deterministically ordered chapter list.**

`DiscoverChaptersCore` performs the full chapter discovery pipeline: enumerate top-level `*.wav` files, sort them numeric-aware (`ChapterFileComparer.Instance`), optionally map stems to `bookIndex` sections (`SectionLocator.ResolveSectionByTitle`), and produce `ChapterInfo` records with de-duplicated matched titles. It builds a section-order lookup from `bookIndex.Sections` to prioritize matched chapters, then sorts final results by book order for matched entries and `CompareStemStrings` for unmatched ones. Enumeration errors and empty-file cases are handled by returning `Array.Empty<ChapterInfo>()`. The method preserves stable stem-based fallback behavior when index matching is unavailable.


#### [[ChapterDiscoveryService.DiscoverChaptersCore]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyList<ChapterInfo> DiscoverChaptersCore(string rootPath, BookIndex bookIndex)
```

**Calls ->**
- [[SectionLocator.ResolveSectionByTitle]]
- [[Ams.Core.Runtime.Chapter.ChapterDiscoveryService.ChapterFileComparer.CompareStemStrings]]

**Called-by <-**
- [[ChapterDiscoveryService.DiscoverChapters_2]]
- [[ChapterDiscoveryService.DiscoverChapters]]


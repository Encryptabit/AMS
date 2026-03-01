---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterDiscoveryService.cs"
access_modifier: "private"
complexity: 14
fan_in: 2
fan_out: 2
tags:
  - method
---
# ChapterDiscoveryService::DiscoverChaptersCore
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterDiscoveryService.cs`


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


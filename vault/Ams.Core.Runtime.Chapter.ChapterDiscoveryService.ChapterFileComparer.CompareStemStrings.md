---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterDiscoveryService.cs"
access_modifier: "internal"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
---
# ChapterFileComparer::CompareStemStrings
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterDiscoveryService.cs`


#### [[Ams.Core.Runtime.Chapter.ChapterDiscoveryService.ChapterFileComparer.CompareStemStrings]]
##### What it does:
<member name="M:Ams.Core.Runtime.Chapter.ChapterDiscoveryService.ChapterFileComparer.CompareStemStrings(System.String,System.String)">
    <summary>
    Compares two stem strings using numeric-aware logic.
    Used when FileInfo is not available.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static int CompareStemStrings(string x, string y)
```

**Calls ->**
- [[Ams.Core.Runtime.Chapter.ChapterDiscoveryService.ChapterFileComparer.GetStemSortKey]]

**Called-by <-**
- [[ChapterDiscoveryService.DiscoverChaptersCore]]


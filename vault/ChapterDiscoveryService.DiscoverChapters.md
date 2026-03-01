---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterDiscoveryService.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 2
tags:
  - method
---
# ChapterDiscoveryService::DiscoverChapters
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterDiscoveryService.cs`


#### [[ChapterDiscoveryService.DiscoverChapters]]
##### What it does:
<member name="M:Ams.Core.Runtime.Chapter.ChapterDiscoveryService.DiscoverChapters(System.String)">
    <summary>
    Discovers chapters by scanning for WAV files and matching them to book index sections.
    </summary>
    <param name="rootPath">Directory containing WAV files and book-index.json.</param>
    <returns>
    List of discovered chapters, sorted by book index order (matched chapters first),
    then by numeric-aware file name sorting for unmatched chapters.
    </returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IReadOnlyList<ChapterInfo> DiscoverChapters(string rootPath)
```

**Calls ->**
- [[ChapterDiscoveryService.DiscoverChaptersCore]]
- [[ChapterDiscoveryService.LoadBookIndex]]

**Called-by <-**
- [[BlazorWorkspace.LoadChaptersFromIndex]]


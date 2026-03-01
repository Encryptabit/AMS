---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterDiscoveryService.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
---
# ChapterDiscoveryService::DiscoverChapters
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterDiscoveryService.cs`


#### [[ChapterDiscoveryService.DiscoverChapters_2]]
##### What it does:
<member name="M:Ams.Core.Runtime.Chapter.ChapterDiscoveryService.DiscoverChapters(System.String,Ams.Core.Runtime.Book.BookIndex)">
    <summary>
    Discovers chapters using a pre-loaded book index.
    </summary>
    <param name="rootPath">Directory containing WAV files.</param>
    <param name="bookIndex">Book index for section matching, or null for stem-only discovery.</param>
    <returns>
    List of discovered chapters, sorted by book index order (matched chapters first),
    then by numeric-aware file name sorting for unmatched chapters.
    </returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IReadOnlyList<ChapterInfo> DiscoverChapters(string rootPath, BookIndex bookIndex)
```

**Calls ->**
- [[ChapterDiscoveryService.DiscoverChaptersCore]]


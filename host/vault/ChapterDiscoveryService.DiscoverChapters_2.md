---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDiscoveryService.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/entry-point
  - llm/data-access
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# ChapterDiscoveryService::DiscoverChapters
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDiscoveryService.cs`

## Summary
**Discovers chapter files from a directory using an optional preloaded book index for title matching and ordering.**

`DiscoverChapters(string rootPath, BookIndex? bookIndex)` is the overload that skips index-file loading and consumes a caller-provided index for section matching. It validates `rootPath` (`ThrowIfNullOrEmpty`), returns `Array.Empty<ChapterInfo>()` when the directory does not exist, and otherwise delegates to `DiscoverChaptersCore(rootPath, bookIndex)` for WAV scanning, matching, and sorting. Passing `null` `bookIndex` intentionally enables stem-only discovery behavior per docs.


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


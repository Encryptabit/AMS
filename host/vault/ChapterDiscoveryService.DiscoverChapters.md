---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDiscoveryService.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 2
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
**Discovers chapter WAVs for a root directory by loading optional book index metadata and delegating to core chapter discovery.**

`DiscoverChapters(string rootPath)` is the public entry overload that performs input/root checks, loads `book-index.json` if available, and delegates discovery/sorting to `DiscoverChaptersCore`. It throws for null/empty `rootPath` (`ThrowIfNullOrEmpty`), returns `Array.Empty<ChapterInfo>()` when the directory is missing, then calls `LoadBookIndex(rootPath)` and forwards both values to core discovery logic. This keeps filesystem probing/index loading separate from the chapter matching/sorting algorithm.


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


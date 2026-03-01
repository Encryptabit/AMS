---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Common/ChapterLabelResolver.cs"
access_modifier: "public"
complexity: 4
fan_in: 7
fan_out: 0
tags:
  - method
---
# ChapterLabelResolver::EnumerateLabelCandidates
**Path**: `home/cari/repos/AMS/host/Ams.Core/Common/ChapterLabelResolver.cs`


#### [[ChapterLabelResolver.EnumerateLabelCandidates]]
##### What it does:
<member name="M:Ams.Core.Common.ChapterLabelResolver.EnumerateLabelCandidates(System.String,System.String)">
    <summary>
    Enumerates candidate labels for section matching from a chapter's identifiers.
    Yields the chapter ID first, then the root directory name.
    </summary>
    <param name="chapterId">The chapter identifier (e.g., "Chapter 01").</param>
    <param name="rootPath">The chapter root path (e.g., "C:\Books\01_Chapter").</param>
    <returns>Label candidates in priority order.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IEnumerable<string> EnumerateLabelCandidates(string chapterId, string rootPath)
```

**Called-by <-**
- [[ChapterContext.GetOrResolveSection]]
- [[ChapterLabelResolverTests.EnumerateLabelCandidates_BothNull_YieldsEmpty]]
- [[ChapterLabelResolverTests.EnumerateLabelCandidates_BothProvided_YieldsChapterIdFirst]]
- [[ChapterLabelResolverTests.EnumerateLabelCandidates_OnlyChapterId_YieldsSingle]]
- [[ChapterLabelResolverTests.EnumerateLabelCandidates_OnlyRootPath_YieldsDirectoryName]]
- [[ChapterLabelResolverTests.EnumerateLabelCandidates_TrailingSeparator_StripsCorrectly]]
- [[ChapterLabelResolverTests.EnumerateLabelCandidates_WhitespaceOnly_YieldsEmpty]]


---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Common/ChapterLabelResolver.cs"
access_modifier: "public"
complexity: 4
fan_in: 4
fan_out: 0
tags:
  - method
---
# ChapterLabelResolver::TryExtractChapterNumber
**Path**: `home/cari/repos/AMS/host/Ams.Core/Common/ChapterLabelResolver.cs`


#### [[ChapterLabelResolver.TryExtractChapterNumber]]
##### What it does:
<member name="M:Ams.Core.Common.ChapterLabelResolver.TryExtractChapterNumber(System.String,System.Int32@)">
    <summary>
    Attempts to extract a chapter number from a label string.
    Handles patterns like "03_2_Title" or "05-12 Something" extracting the second number.
    </summary>
    <param name="label">The label to parse (e.g., directory name or chapter ID).</param>
    <param name="number">The extracted chapter number if successful.</param>
    <returns>True if a chapter number was successfully extracted.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static bool TryExtractChapterNumber(string label, out int number)
```

**Called-by <-**
- [[ChapterContext.GetOrResolveSection]]
- [[ChapterLabelResolverTests.TryExtractChapterNumber_InvalidPatterns_ReturnsFalse]]
- [[ChapterLabelResolverTests.TryExtractChapterNumber_NoWordBoundary_ReturnsFalse]]
- [[ChapterLabelResolverTests.TryExtractChapterNumber_ValidPatterns_ReturnsTrue]]


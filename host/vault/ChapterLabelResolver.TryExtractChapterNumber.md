---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Common/ChapterLabelResolver.cs"
access_modifier: "public"
complexity: 4
fan_in: 4
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# ChapterLabelResolver::TryExtractChapterNumber
**Path**: `Projects/AMS/host/Ams.Core/Common/ChapterLabelResolver.cs`

## Summary
**Parse labels in prefixed-number formats and extract the chapter number component in a safe `Try*` pattern.**

`TryExtractChapterNumber(string label, out int number)` attempts structured chapter-number parsing using a compiled regex `^\s*\d+\s*[_-]\s*(\d+)\b`, which captures the second numeric token after an underscore or hyphen. It initializes `number = 0`, rejects null/whitespace labels early, then runs `ChapterNumberPattern.Match(label)`. On regex success, it parses capture group 1 via `int.TryParse` and returns `true` with the parsed value; otherwise it returns `false` and leaves `number` as `0`.


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


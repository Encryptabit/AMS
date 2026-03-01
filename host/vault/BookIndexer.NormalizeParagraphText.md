---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# BookIndexer::NormalizeParagraphText
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

## Summary
**Cleans paragraph text by collapsing forced hyphenated line breaks into continuous words.**

`NormalizeParagraphText` normalizes paragraph content by removing forced hyphen line-break artifacts while preserving other text. It returns `string.Empty` for null/empty input, otherwise applies `ForcedHyphenBreakRegex.Replace(text, string.Empty)`. The regex targets letter-hyphen-newline-letter patterns, effectively joining words that were split across wrapped lines.


#### [[BookIndexer.NormalizeParagraphText]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string NormalizeParagraphText(string text)
```

**Called-by <-**
- [[BookIndexer.BuildParagraphTexts]]


---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# BookIndexer::NormalizeHeadingArtifacts
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

## Summary
**Cleans and canonicalizes heading text, converting OCR-like chapter headings into a normalized “CHAPTER N” form.**

`NormalizeHeadingArtifacts` normalizes noisy heading text (especially OCR chapter headers) into a canonical chapter form when possible. It returns `string.Empty` for null/whitespace input, otherwise trims, uppercases, removes whitespace for matching, extracts digit characters, and applies `OcrChapterHeaderRegex` to detect chapter-style patterns. When matched, it emits `CHAPTER {number}` using extracted digits (or regex group fallback); if not matched, it returns the trimmed original text.


#### [[BookIndexer.NormalizeHeadingArtifacts]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string NormalizeHeadingArtifacts(string text)
```

**Called-by <-**
- [[BookIndexer.Process]]


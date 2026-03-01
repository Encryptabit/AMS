---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 12
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# BookIndexer::ApplyChapterDuplicateSuffixes
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

## Summary
**Disambiguates repeated chapter section titles by deterministically appending letter suffixes within duplicate groups.**

`ApplyChapterDuplicateSuffixes` rewrites duplicate chapter titles by appending alphabetic suffixes (`A`, `B`, …) to distinguish repeated headings. It first filters `sections` to chapter-kind entries whose titles match `ChapterDuplicateRegex` and currently lack an existing suffix, then groups candidates by normalized base title (`prefix+ws+number`, uppercased) and level. Only groups with multiple entries and identical original titles are rewritten; each item is ordered by `StartWord` and updated in-place via record `with` assignment (`Title = prefix + ws + number + suffixLetter`). The method no-ops for null/empty lists and for non-ambiguous groups.


#### [[BookIndexer.ApplyChapterDuplicateSuffixes]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void ApplyChapterDuplicateSuffixes(List<SectionRange> sections)
```

**Called-by <-**
- [[BookIndexer.Process]]


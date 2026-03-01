---
namespace: "Ams.Core.Application.Commands"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Commands/MergeTimingsCommand.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# MergeTimingsCommand::EnumerateChapterLabels
**Path**: `Projects/AMS/host/Ams.Core/Application/Commands/MergeTimingsCommand.cs`

## Summary
**It generates normalized, non-empty chapter label candidates from descriptor metadata for downstream section resolution.**

`EnumerateChapterLabels` is an iterator that yields candidate chapter identifiers for section matching in priority order. It emits `chapter.Descriptor.ChapterId` if non-empty, then each non-empty value from `chapter.Descriptor.Aliases` (or `Array.Empty<string>()` when null), and finally a directory-name label derived from `chapter.Descriptor.RootPath` after trimming trailing separators and applying `Path.GetFileName`. All outputs are filtered with `string.IsNullOrWhiteSpace` checks, producing only usable labels.


#### [[MergeTimingsCommand.EnumerateChapterLabels]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IEnumerable<string> EnumerateChapterLabels(ChapterContext chapter)
```

**Called-by <-**
- [[MergeTimingsCommand.ResolveChapterWordWindow]]


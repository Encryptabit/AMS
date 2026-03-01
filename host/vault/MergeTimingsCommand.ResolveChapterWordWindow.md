---
namespace: "Ams.Core.Application.Commands"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Commands/MergeTimingsCommand.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# MergeTimingsCommand::ResolveChapterWordWindow
**Path**: `Projects/AMS/host/Ams.Core/Application/Commands/MergeTimingsCommand.cs`

## Summary
**It resolves and bounds the effective start/end word indices for a chapter within the book index.**

`ResolveChapterWordWindow` determines the chapter’s book-word bounds by first using `chapter.Descriptor.BookStartWord/BookEndWord`, then falling back (if either is missing) to section lookup via `EnumerateChapterLabels(chapter)` and `SectionLocator.ResolveSectionByTitle(bookIndex, label)`. When a matching section is found, it adopts `section.StartWord`/`section.EndWord`; otherwise it defaults to the full index span. It normalizes results against `bookIndex.Words.Length - 1` using `Math.Clamp`, ensuring `startWord` is in range and `endWord` is clamped to `[startWord, maxIndex]`.


#### [[MergeTimingsCommand.ResolveChapterWordWindow]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static (int startWord, int endWord) ResolveChapterWordWindow(ChapterContext chapter, BookIndex bookIndex)
```

**Calls ->**
- [[MergeTimingsCommand.EnumerateChapterLabels]]
- [[SectionLocator.ResolveSectionByTitle]]

**Called-by <-**
- [[MergeTimingsCommand.ExecuteAsync]]


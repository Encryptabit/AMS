---
namespace: "Ams.Core.Application.Commands"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Commands/MergeTimingsCommand.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 2
tags:
  - method
---
# MergeTimingsCommand::ResolveChapterWordWindow
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Commands/MergeTimingsCommand.cs`


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


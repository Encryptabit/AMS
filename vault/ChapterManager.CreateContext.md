---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "public"
complexity: 17
fan_in: 3
fan_out: 8
tags:
  - method
  - danger/high-complexity
---
# ChapterManager::CreateContext
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

> [!danger] High Complexity (17)
> Cyclomatic complexity: 17. Consider refactoring into smaller methods.


#### [[ChapterManager.CreateContext]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ChapterContextHandle CreateContext(FileInfo bookIndexFile, FileInfo asrFile = null, FileInfo transcriptFile = null, FileInfo hydrateFile = null, FileInfo audioFile = null, DirectoryInfo chapterDirectory = null, string chapterId = null, bool reloadBookIndex = false)
```

**Calls ->**
- [[AsrTranscriptBuilder.BuildCorpusText]]
- [[BookDocuments.SetLoadedBookIndex]]
- [[ChapterManager.BuildAliasSet]]
- [[ChapterManager.DetermineChapterStem]]
- [[ChapterManager.EnsureChapterDescriptor]]
- [[ChapterManager.Load_2]]
- [[ChapterManager.LoadJson]]
- [[ChapterManager.ResolveChapterRoot]]

**Called-by <-**
- [[CliWorkspace.OpenChapter]]
- [[ChapterContextHandle.Create]]
- [[BlazorWorkspace.OpenChapter]]


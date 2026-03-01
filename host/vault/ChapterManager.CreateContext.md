---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "public"
complexity: 17
fan_in: 3
fan_out: 8
tags:
  - method
  - danger/high-complexity
  - llm/entry-point
  - llm/factory
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# ChapterManager::CreateContext
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

> [!danger] High Complexity (17)
> Cyclomatic complexity: 17. Consider refactoring into smaller methods.

## Summary
**Creates and initializes a chapter runtime context handle from index/audio/artifact inputs, wiring descriptor metadata and optional preloaded documents.**

`CreateContext` bootstraps a runtime chapter context from artifact files by validating `bookIndexFile`, deriving chapter identity/root (`DetermineChapterStem`, `ResolveChapterRoot`), loading book index state (cached or `LoadJson<BookIndex>`), and building alias/section linkage (`BuildAliasSet`). It synthesizes audio buffer descriptors (`raw`, `treated`, `corrected`, `filtered`), upserts/ensures the chapter descriptor, loads the chapter context, and conditionally injects preexisting ASR/transcript/hydrate documents when provided files exist. It also ensures book-index state is set in `BookDocuments` when needed (`SetLoadedBookIndex`) and backfills ASR corpus text when missing. The method returns a new `ChapterContextHandle` over the resolved book/chapter contexts.


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


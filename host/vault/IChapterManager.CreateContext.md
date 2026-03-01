---
namespace: "Ams.Core.Runtime.Interfaces"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Interfaces/IChapterManager.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/entry-point
  - llm/factory
  - llm/di
  - llm/data-access
  - llm/validation
---
# IChapterManager::CreateContext
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Interfaces/IChapterManager.cs`

## Summary
**Declares the chapter manager API for creating a `ChapterContextHandle` from chapter/book artifact inputs.**

`CreateContext(...)` is an `IChapterManager` interface contract for constructing a chapter runtime handle from artifact file inputs and optional overrides (ASR/transcript/hydrate/audio paths, chapter directory/ID, reload behavior). It defines the creation API shape only; resolution, validation, and loading logic are implementation-specific. This is the high-level context bootstrap entry point of the chapter manager abstraction.


#### [[IChapterManager.CreateContext]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
ChapterContextHandle CreateContext(FileInfo bookIndexFile, FileInfo asrFile = null, FileInfo transcriptFile = null, FileInfo hydrateFile = null, FileInfo audioFile = null, DirectoryInfo chapterDirectory = null, string chapterId = null, bool reloadBookIndex = false)
```


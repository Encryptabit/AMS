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
  - llm/di
  - llm/utility
---
# IChapterManager::Load
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Interfaces/IChapterManager.cs`

## Summary
**Declares the chapter manager API for loading a chapter context by index.**

`Load(int index)` is an `IChapterManager` interface contract for positional chapter-context retrieval. It defines API shape only (no implementation body), so bounds checking, cache behavior, and failure semantics are implementation-defined. This method is a core chapter navigation entry in the manager abstraction.


#### [[IChapterManager.Load]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
ChapterContext Load(int index)
```


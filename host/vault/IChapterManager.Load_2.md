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
**Declares the chapter manager API for loading a chapter context by chapter ID.**

`Load(string chapterId)` is an `IChapterManager` interface contract for chapter-context retrieval by identifier. It provides no implementation logic in the interface, so matching rules, normalization, and missing-ID behavior are delegated to concrete chapter manager implementations. This method complements index-based loading in the chapter manager API.


#### [[IChapterManager.Load_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
ChapterContext Load(string chapterId)
```


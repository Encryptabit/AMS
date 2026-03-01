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
# IChapterManager::TryMovePrevious
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Interfaces/IChapterManager.cs`

## Summary
**Declares the chapter manager API for attempting non-throwing movement to the previous chapter context.**

`TryMovePrevious(out ChapterContext context)` is an `IChapterManager` interface contract for backward navigation with non-throwing boolean semantics. It specifies API shape only, leaving cursor logic, boundary behavior, and out-parameter policy to concrete implementations. This method complements `TryMoveNext` in the traversal surface.


#### [[IChapterManager.TryMovePrevious]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
bool TryMovePrevious(out ChapterContext context)
```


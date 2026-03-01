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
# IChapterManager::TryMoveNext
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Interfaces/IChapterManager.cs`

## Summary
**Declares the chapter manager API for attempting non-throwing movement to the next chapter context.**

`TryMoveNext(out ChapterContext context)` is an `IChapterManager` interface contract for forward chapter traversal using non-throwing boolean semantics. It defines method shape only, with cursor behavior, boundary handling, and out-value policy delegated to implementations. This method is part of the chapter manager’s navigation API.


#### [[IChapterManager.TryMoveNext]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
bool TryMoveNext(out ChapterContext context)
```


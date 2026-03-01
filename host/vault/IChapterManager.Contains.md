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
# IChapterManager::Contains
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Interfaces/IChapterManager.cs`

## Summary
**Declares the chapter manager API for testing whether a chapter ID is available.**

`Contains(string chapterId)` is an `IChapterManager` interface contract for checking chapter presence by identifier. It has no implementation body here, so ID normalization and lookup semantics are determined by concrete managers. The method provides a boolean existence query in the chapter manager API.


#### [[IChapterManager.Contains]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
bool Contains(string chapterId)
```


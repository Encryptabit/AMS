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
  - llm/validation
---
# IChapterManager::UpsertDescriptor
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Interfaces/IChapterManager.cs`

## Summary
**Declares the chapter manager API for adding or updating a chapter descriptor.**

`UpsertDescriptor(ChapterDescriptor descriptor)` is an `IChapterManager` interface contract for mutating chapter descriptor state via insert-or-update semantics. It contains no implementation logic here, so merge strategy, identity matching, and validation/error behavior are defined by concrete managers. The method provides descriptor-management capability on top of context navigation APIs.


#### [[IChapterManager.UpsertDescriptor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
ChapterDescriptor UpsertDescriptor(ChapterDescriptor descriptor)
```


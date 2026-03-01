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
# IChapterManager::Reset
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Interfaces/IChapterManager.cs`

## Summary
**Declares the chapter manager API for resetting chapter navigation state.**

`Reset()` is an `IChapterManager` interface contract method for restoring manager navigation state to an implementation-defined start position. It has no implementation body at the interface layer, so concrete managers define exact cursor/side-effect behavior. This method belongs to the chapter traversal lifecycle surface.


#### [[IChapterManager.Reset]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
void Reset()
```


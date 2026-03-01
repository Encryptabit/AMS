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
# IChapterManager::DeallocateAll
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Interfaces/IChapterManager.cs`

## Summary
**Declares the chapter manager API for deallocating every managed chapter context.**

`DeallocateAll()` is an `IChapterManager` interface contract that defines bulk deallocation of all managed chapter contexts/resources. As an interface member, it contains no executable logic and leaves teardown semantics to concrete implementations. It serves as the aggregate cleanup operation in the chapter manager API.


#### [[IChapterManager.DeallocateAll]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
void DeallocateAll()
```


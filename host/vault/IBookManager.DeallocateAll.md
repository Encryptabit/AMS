---
namespace: "Ams.Core.Runtime.Interfaces"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Interfaces/IBookManager.cs"
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
# IBookManager::DeallocateAll
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Interfaces/IBookManager.cs`

## Summary
**Declares the book manager API for deallocating all managed book contexts.**

`DeallocateAll()` is an `IBookManager` interface contract for bulk release of all managed book contexts/resources. It has no implementation logic at the interface level, so concrete managers define exact teardown, save, and cache-clear semantics. This method completes the deallocation lifecycle surface of the manager API.


#### [[IBookManager.DeallocateAll]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
void DeallocateAll()
```


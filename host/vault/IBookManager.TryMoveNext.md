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
# IBookManager::TryMoveNext
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Interfaces/IBookManager.cs`

## Summary
**Declares the book manager API for attempting non-throwing movement to the next book context.**

`TryMoveNext(out BookContext context)` is an `IBookManager` interface contract for forward traversal with non-throwing boolean semantics. It defines that implementations should attempt movement and return a context via the out parameter, while internal cursor rules and boundary behavior remain implementation-defined. This method is part of the manager’s navigation API surface.


#### [[IBookManager.TryMoveNext]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
bool TryMoveNext(out BookContext context)
```


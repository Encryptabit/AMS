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
# IBookManager::TryMovePrevious
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Interfaces/IBookManager.cs`

## Summary
**Declares the book manager API for attempting non-throwing movement to the previous book context.**

`TryMovePrevious(out BookContext context)` is an `IBookManager` interface contract for backward traversal using non-throwing boolean semantics. It defines API shape only (no implementation body), so cursor handling, boundary behavior, and what context is returned on failure are determined by concrete managers. This method complements `TryMoveNext` in the navigation surface.


#### [[IBookManager.TryMovePrevious]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
bool TryMovePrevious(out BookContext context)
```


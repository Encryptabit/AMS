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
# IBookManager::Reset
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Interfaces/IBookManager.cs`

## Summary
**Declares the book manager API for resetting internal navigation state.**

`Reset()` is an `IBookManager` interface contract method for resetting manager navigation/state to its implementation-defined initial position. It contains no implementation logic in the interface, so concrete managers define exact reset side effects. The method belongs to the manager’s traversal lifecycle API.


#### [[IBookManager.Reset]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
void Reset()
```


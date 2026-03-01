---
namespace: "Ams.Core.Runtime.Interfaces"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Interfaces/IAudioBufferManager.cs"
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
# IAudioBufferManager::TryMoveNext
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Interfaces/IAudioBufferManager.cs`

## Summary
**Declares the audio buffer manager API for attempting non-throwing movement to the next buffer context.**

`TryMoveNext(out AudioBufferContext bufferContext)` is an `IAudioBufferManager` interface contract for forward navigation without exception-driven control flow. It defines a boolean success/failure result with an out context payload but leaves cursor rules and boundary behavior to implementations. The method forms part of the manager’s traversal API surface.


#### [[IAudioBufferManager.TryMoveNext]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
bool TryMoveNext(out AudioBufferContext bufferContext)
```


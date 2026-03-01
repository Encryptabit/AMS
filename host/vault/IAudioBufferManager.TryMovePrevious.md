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
# IAudioBufferManager::TryMovePrevious
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Interfaces/IAudioBufferManager.cs`

## Summary
**Declares the audio buffer manager API for attempting non-throwing movement to the previous buffer context.**

`TryMovePrevious(out AudioBufferContext bufferContext)` is an `IAudioBufferManager` interface contract for backward navigation with boolean success semantics. It specifies an out-parameter context return shape while leaving boundary handling and cursor mechanics to concrete implementations. This method complements `TryMoveNext` in the traversal API.


#### [[IAudioBufferManager.TryMovePrevious]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
bool TryMovePrevious(out AudioBufferContext bufferContext)
```


---
namespace: "Ams.Core.Runtime.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AudioBufferManager::Deallocate
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs`

## Summary
**Removes a specific buffer context from cache and unloads it if present.**

`Deallocate` conditionally evicts a cached buffer context by ID and releases its resources. It first short-circuits on null/whitespace `bufferId`, then calls `_cache.Remove(bufferId, out var context)`; on success it invokes `context.Unload()` and emits a debug log with the removed ID and new cache count. Missing IDs are treated as no-op, so the method is safe to call repeatedly.


#### [[AudioBufferManager.Deallocate]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Deallocate(string bufferId)
```

**Calls ->**
- [[Log.Debug]]
- [[AudioBufferContext.Unload]]

**Called-by <-**
- [[PolishService.PersistCorrectedBuffer]]


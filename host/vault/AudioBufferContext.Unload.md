---
namespace: "Ams.Core.Runtime.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Audio/AudioBufferContext.cs"
access_modifier: "public"
complexity: 1
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# AudioBufferContext::Unload
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Audio/AudioBufferContext.cs`

## Summary
**Reset the context’s loaded audio buffer state and log that the buffer has been unloaded.**

`Unload` clears the lazy-load cache in `AudioBufferContext` by setting `_buffer = null` and `_loaded = false`. This forces the next `Buffer` access to re-run `_loader(_descriptor)` instead of returning a previously loaded instance. It then emits a diagnostic message through `Log.Debug("AudioBufferContext unloaded buffer {BufferId}", _descriptor.BufferId)` for lifecycle tracing.


#### [[AudioBufferContext.Unload]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Unload()
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[AudioBufferManager.Deallocate]]
- [[AudioBufferManager.DeallocateAll]]


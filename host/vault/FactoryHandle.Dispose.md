---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "public"
complexity: 4
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/factory
  - llm/error-handling
  - llm/utility
---
# FactoryHandle::Dispose
**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

## Summary
**Returns a borrowed factory handle to the pool, disposing and evicting the shared factory when its reference count drops to zero.**

`Dispose` is the release path for a pooled `WhisperFactory`: it first short-circuits if `_entry` is already null (idempotent dispose), then locks on `SyncRoot`, decrements `_entry.RefCount`, and when it reaches zero removes the keyed entry from `Entries` and disposes the underlying `WhisperFactory`. After lock-protected release, it nulls `_entry` to prevent double-release on subsequent calls. This implements reference-counted pool eviction with thread-safe teardown.


#### [[FactoryHandle.Dispose]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Dispose()
```


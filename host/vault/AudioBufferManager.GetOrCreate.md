---
namespace: "Ams.Core.Runtime.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/factory
  - llm/utility
---
# AudioBufferManager::GetOrCreate
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs`

## Summary
**Retrieves an existing buffer context from cache or lazily creates and caches one for a descriptor.**

`GetOrCreate` provides cache-backed retrieval of `AudioBufferContext` keyed by `descriptor.BufferId`. It checks `_cache.TryGetValue`; on miss, it creates a new `AudioBufferContext(descriptor, _loader)`, stores it in `_cache`, and logs creation; on hit, it logs reuse. The method always returns the cached/new context, centralizing lazy instantiation and reuse semantics for callers.


#### [[AudioBufferManager.GetOrCreate]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private AudioBufferContext GetOrCreate(AudioBufferDescriptor descriptor)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[AudioBufferManager.Load]]
- [[AudioBufferManager.Load_2]]


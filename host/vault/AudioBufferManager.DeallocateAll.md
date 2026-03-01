---
namespace: "Ams.Core.Runtime.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs"
access_modifier: "public"
complexity: 3
fan_in: 3
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# AudioBufferManager::DeallocateAll
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs`

## Summary
**Unloads and removes all cached audio buffer contexts in a single cache-flush operation.**

`DeallocateAll` drains the buffer-context cache by unloading every cached `AudioBufferContext` and then clearing the dictionary. It iterates `_cache.Values`, invokes `context.Unload()` for each entry, conditionally logs a debug message when at least one item existed, and finally calls `_cache.Clear()`. This performs explicit resource teardown while leaving descriptor metadata and cursor state unchanged.


#### [[AudioBufferManager.DeallocateAll]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void DeallocateAll()
```

**Calls ->**
- [[Log.Debug]]
- [[AudioBufferContext.Unload]]

**Called-by <-**
- [[ChapterManager.Deallocate]]
- [[ChapterManager.DeallocateAll]]
- [[ChapterManager.EnsureCapacity]]


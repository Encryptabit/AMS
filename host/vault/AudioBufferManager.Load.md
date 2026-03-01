---
namespace: "Ams.Core.Runtime.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs"
access_modifier: "public"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AudioBufferManager::Load
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs`

## Summary
**Moves the active buffer cursor to a specific descriptor index and returns the corresponding audio buffer context.**

`Load(int index)` validates the requested position against `_descriptors.Count` using an unsigned range check (`(uint)index >= (uint)_descriptors.Count`) and throws `ArgumentOutOfRangeException` when invalid. On success it updates the manager cursor (`_cursor = index`), resolves the matching `AudioBufferDescriptor`, and delegates to `GetOrCreate(descriptor)`. That delegation returns a cached `AudioBufferContext` or lazily constructs one, so this method also drives cache-backed context reuse/creation.


#### [[AudioBufferManager.Load]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public AudioBufferContext Load(int index)
```

**Calls ->**
- [[AudioBufferManager.GetOrCreate]]

**Called-by <-**
- [[AudioBufferManager.TryMoveNext]]
- [[AudioBufferManager.TryMovePrevious]]


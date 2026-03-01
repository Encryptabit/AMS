---
namespace: "Ams.Core.Runtime.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Audio/AudioBufferContext.cs"
access_modifier: "internal"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/di
---
# AudioBufferContext::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Audio/AudioBufferContext.cs`

## Summary
**Initialize an audio-buffer context with descriptor metadata and a deferred loader function for lazy buffer retrieval.**

The internal `AudioBufferContext` constructor stores two dependencies: an `AudioBufferDescriptor` and a loader delegate `Func<AudioBufferDescriptor, AudioBuffer?>` into private readonly fields (`_descriptor`, `_loader`). It performs no eager loading, validation, or side effects; buffer materialization is deferred to the `Buffer` getter, which invokes `_loader(_descriptor)` on first access and tracks state via `_loaded`/`_buffer`.


#### [[AudioBufferContext..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal AudioBufferContext(AudioBufferDescriptor descriptor, Func<AudioBufferDescriptor, AudioBuffer> loader)
```


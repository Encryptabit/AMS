---
namespace: "Ams.Core.Runtime.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/di
  - llm/validation
---
# AudioBufferManager::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs`

## Summary
**Construct an audio-buffer manager with descriptor metadata, a pluggable or default buffer loader, and empty runtime cache state.**

The `AudioBufferManager` constructor initializes core manager state by normalizing optional dependencies and preparing cache/navigation fields. It assigns `_descriptors` to the provided list or `Array.Empty<AudioBufferDescriptor>()` when null, assigns `_loader` to the injected delegate or falls back to `DefaultLoader`, and creates `_cache` as a case-insensitive dictionary keyed by `BufferId` (`StringComparer.OrdinalIgnoreCase`). It also resets `_cursor` to `0`, establishing deterministic initial position for subsequent `Load`/navigation calls.


#### [[AudioBufferManager..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public AudioBufferManager(IReadOnlyList<AudioBufferDescriptor> descriptors, Func<AudioBufferDescriptor, AudioBuffer> loader = null)
```


---
namespace: "Ams.Core.Runtime.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "Ams.Core.Runtime.Interfaces.IAudioBufferManager"
member_count: 10
dependency_count: 0
pattern: ~
tags:
  - class
---

# AudioBufferManager

> Class in `Ams.Core.Runtime.Audio`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs`

**Implements**:
- [[IAudioBufferManager]]

## Properties
- `Count`: int
- `Current`: AudioBufferContext
- `_descriptors`: IReadOnlyList<AudioBufferDescriptor>
- `_cache`: Dictionary<string, AudioBufferContext>
- `_loader`: Func<AudioBufferDescriptor, AudioBuffer?>
- `_cursor`: int

## Members
- [[AudioBufferManager..ctor]]
- [[AudioBufferManager.Load]]
- [[AudioBufferManager.Load_2]]
- [[AudioBufferManager.TryMoveNext]]
- [[AudioBufferManager.TryMovePrevious]]
- [[AudioBufferManager.Reset]]
- [[AudioBufferManager.Deallocate]]
- [[AudioBufferManager.DeallocateAll]]
- [[AudioBufferManager.GetOrCreate]]
- [[AudioBufferManager.DefaultLoader]]


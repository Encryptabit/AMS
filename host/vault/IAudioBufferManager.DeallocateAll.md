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
# IAudioBufferManager::DeallocateAll
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Interfaces/IAudioBufferManager.cs`

## Summary
**Declares the audio buffer manager API for deallocating all managed buffer contexts.**

`DeallocateAll()` is an `IAudioBufferManager` interface contract that defines bulk resource/context release semantics for all managed audio buffers. As an interface member, it provides no implementation and leaves cleanup strategy and side effects to concrete managers. It completes the lifecycle-management portion of the audio buffer manager API.


#### [[IAudioBufferManager.DeallocateAll]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
void DeallocateAll()
```


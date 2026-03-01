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
# IAudioBufferManager::Reset
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Interfaces/IAudioBufferManager.cs`

## Summary
**Declares the audio buffer manager API for resetting buffer navigation state.**

`Reset()` is an interface contract member on `IAudioBufferManager` that defines navigation-state reset behavior. It has no implementation here, so concrete managers determine what internal cursor/state is reset and how side effects are handled. The method serves as the canonical “return to initial position” operation in the audio buffer manager API.


#### [[IAudioBufferManager.Reset]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
void Reset()
```


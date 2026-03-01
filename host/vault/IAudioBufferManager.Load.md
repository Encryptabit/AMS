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
# IAudioBufferManager::Load
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Interfaces/IAudioBufferManager.cs`

## Summary
**Declares the audio buffer manager API for loading a buffer context by index.**

`Load(int index)` is an `IAudioBufferManager` interface contract method that defines index-based buffer-context retrieval. As an interface member, it contains no implementation logic and leaves bounds handling, caching, and error behavior to concrete managers. It establishes the positional load API surface alongside string-ID loading and navigation methods.


#### [[IAudioBufferManager.Load]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
AudioBufferContext Load(int index)
```


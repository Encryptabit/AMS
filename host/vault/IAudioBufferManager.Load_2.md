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
**Declares the audio buffer manager API for loading a buffer context by buffer identifier.**

`Load(string bufferId)` is an interface contract method on `IAudioBufferManager` for identifier-based buffer context retrieval. It defines API shape only (no implementation), leaving lookup strategy, caching behavior, and failure semantics to concrete managers. This complements the index-based `Load(int)` entry point.


#### [[IAudioBufferManager.Load_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
AudioBufferContext Load(string bufferId)
```


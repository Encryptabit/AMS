---
namespace: "Ams.Core.Runtime.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs"
access_modifier: "public"
complexity: 3
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
**Loads an audio buffer context by buffer ID, updating cursor state and using cache-backed context retrieval.**

`Load(string bufferId)` resolves an audio buffer context by logical ID with a case-insensitive linear scan over `_descriptors`. It validates input using `ArgumentException.ThrowIfNullOrEmpty`, updates `_cursor` to the matched descriptor index, and returns `GetOrCreate(_descriptors[i])` to reuse/create cached `AudioBufferContext` instances. If no descriptor matches, it throws `KeyNotFoundException` with the missing ID in the message.


#### [[AudioBufferManager.Load_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public AudioBufferContext Load(string bufferId)
```

**Calls ->**
- [[AudioBufferManager.GetOrCreate]]

**Called-by <-**
- [[AsrService.ResolveAudioBufferContext]]
- [[PolishService.PersistCorrectedBuffer]]


---
namespace: "Ams.Core.Runtime.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AudioBufferManager::TryMoveNext
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs`

## Summary
**Tries to advance to the next audio buffer context and reports success without throwing for end-of-sequence cases.**

`TryMoveNext` attempts cursor advancement to the next descriptor while preserving a non-throwing boolean contract. When `_cursor + 1` is out of range, it returns `false` and sets `bufferContext` to `Current` if any descriptors exist (or `null!` when empty). Otherwise it loads the next buffer via `Load(_cursor + 1)`, assigns that context to the out parameter, and returns `true`. The method therefore combines bounds gating with stateful navigation.


#### [[AudioBufferManager.TryMoveNext]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool TryMoveNext(out AudioBufferContext bufferContext)
```

**Calls ->**
- [[AudioBufferManager.Load]]


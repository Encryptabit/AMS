---
namespace: "Ams.Core.Runtime.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs"
access_modifier: "public"
complexity: 4
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AudioBufferManager::TryMovePrevious
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs`

## Summary
**Attempts to move to the previous audio buffer context and indicates whether the move succeeded.**

`TryMovePrevious` provides non-throwing backward navigation over `_descriptors` using cursor bounds checks. If already at the start (`_cursor <= 0`) or the collection is empty, it sets `bufferContext` to `Current` when available (otherwise `null!`) and returns `false`. Otherwise it loads the prior descriptor with `Load(_cursor - 1)`, assigns the resulting context, and returns `true`. The method encapsulates stateful cursor movement with safe boundary behavior.


#### [[AudioBufferManager.TryMovePrevious]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool TryMovePrevious(out AudioBufferContext bufferContext)
```

**Calls ->**
- [[AudioBufferManager.Load]]


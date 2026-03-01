---
namespace: "Ams.Core.Runtime.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
---
# AudioBufferManager::Reset
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs`

## Summary
**Resets audio buffer navigation to the initial cursor position.**

`Reset` is a constant-time state mutator that sets the manager cursor back to the first descriptor position (`_cursor = 0`). It does not touch the descriptor list, cache, or loaded buffer contexts. No validation or side effects beyond cursor reset are performed.


#### [[AudioBufferManager.Reset]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Reset()
```


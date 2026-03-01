---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
---
# ChapterManager::Reset
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

## Summary
**Resets chapter navigation to the first descriptor index.**

`Reset` is a constant-time state reset that sets `_cursor` back to `0`. It does not alter descriptors, cache entries, or usage-order tracking structures. No validation or additional side effects are performed.


#### [[ChapterManager.Reset]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Reset()
```


---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/entry-point
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# ChapterManager::TryMoveNext
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

## Summary
**Attempts to advance to the next chapter context and reports success via a boolean result.**

`TryMoveNext` attempts forward chapter navigation without throwing at bounds. If `_cursor + 1` exceeds available descriptors, it returns `false` and outputs either `Current` (when descriptors exist) or `null!` (when empty); otherwise it loads the next chapter via `Load(_cursor + 1)`, assigns it to the out parameter, and returns `true`. The method encapsulates cursor boundary checks plus stateful advancement.


#### [[ChapterManager.TryMoveNext]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool TryMoveNext(out ChapterContext context)
```

**Calls ->**
- [[ChapterManager.Load]]


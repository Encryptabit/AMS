---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "public"
complexity: 4
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/entry-point
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# ChapterManager::TryMovePrevious
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

## Summary
**Attempts to move to the previous chapter context and indicates whether navigation succeeded.**

`TryMovePrevious` performs non-throwing backward navigation over chapter descriptors with boundary guards. If already at the first item (`_cursor <= 0`) or when no descriptors exist, it returns `false` and outputs `Current` when available (otherwise `null!`); otherwise it loads the previous context via `Load(_cursor - 1)`, assigns it, and returns `true`. This method combines safe bounds handling with cursor-based chapter traversal.


#### [[ChapterManager.TryMovePrevious]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool TryMovePrevious(out ChapterContext context)
```

**Calls ->**
- [[ChapterManager.Load]]


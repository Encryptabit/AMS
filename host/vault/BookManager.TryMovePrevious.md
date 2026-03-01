---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookManager.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# BookManager::TryMovePrevious
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookManager.cs`

## Summary
**Attempts to move to the previous book context and indicates whether the move succeeded.**

`TryMovePrevious` performs non-throwing backward navigation using `_cursor` and descriptor count guards. It returns `false` when already at the beginning (`_cursor <= 0`) or when no descriptors exist, and sets the out value to `Current` when available (otherwise `null!`). If movement is valid, it loads the prior context via `Load(_cursor - 1)`, assigns it, and returns `true`.


#### [[BookManager.TryMovePrevious]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool TryMovePrevious(out BookContext context)
```

**Calls ->**
- [[BookManager.Load]]


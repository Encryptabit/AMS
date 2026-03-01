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
# BookManager::TryMoveNext
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookManager.cs`

## Summary
**Attempts to move to the next book context and reports success without throwing on end-of-sequence.**

`TryMoveNext` implements non-throwing forward navigation across `_descriptors` using the manager cursor. If the next index would exceed bounds, it returns `false` and outputs either `null!` (when no descriptors exist) or `Current`; otherwise it loads the next book via `Load(_cursor + 1)`, assigns it to the out parameter, and returns `true`. The method encapsulates cursor advancement and boundary handling in a boolean API.


#### [[BookManager.TryMoveNext]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool TryMoveNext(out BookContext context)
```

**Calls ->**
- [[BookManager.Load]]


---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookManager.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
---
# BookManager::Reset
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookManager.cs`

## Summary
**Resets the manager’s current book pointer to the first descriptor index.**

`Reset` is a constant-time state mutator that reinitializes book navigation by setting `_cursor` to `0`. It does not modify descriptor data, resolver state, or cached `BookContext` instances. No validation or side effects beyond cursor reset are performed.


#### [[BookManager.Reset]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Reset()
```


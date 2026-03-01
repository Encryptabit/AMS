---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookDocuments.cs"
access_modifier: "internal"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/data-access
---
# BookDocuments::SetLoadedBookIndex
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookDocuments.cs`

## Summary
**Injects a loaded book index into the document slot and marks it as already persisted.**

`SetLoadedBookIndex` is an internal state-injection helper that writes a preloaded `BookIndex` into the `_bookIndex` document slot. It delegates directly to `_bookIndex.SetValue(bookIndex, markClean: true)`, explicitly marking the slot as clean so no save is required immediately. The method bypasses load-on-demand semantics while preserving dirty-state correctness.


#### [[BookDocuments.SetLoadedBookIndex]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal void SetLoadedBookIndex(BookIndex bookIndex)
```

**Calls ->**
- [[DocumentSlot_T_.SetValue_2]]

**Called-by <-**
- [[ChapterManager.CreateContext]]


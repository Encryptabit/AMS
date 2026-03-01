---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookContext.cs"
access_modifier: "public"
complexity: 1
fan_in: 3
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/data-access
---
# BookContext::Save
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookContext.cs`

## Summary
**Commits pending book document changes through the context’s document manager.**

`Save` is a thin façade on `BookContext` that delegates persistence to the document subsystem. Its entire implementation is `Documents.SaveChanges();`, so it does not perform local validation, batching, or error handling. Any write behavior and exceptions come from `BookDocuments.SaveChanges`.


#### [[BookContext.Save]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Save()
```

**Calls ->**
- [[BookDocuments.SaveChanges]]

**Called-by <-**
- [[BookManager.Deallocate]]
- [[BookManager.DeallocateAll]]
- [[ChapterContextHandle.Save]]


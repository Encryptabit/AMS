---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookContext.cs"
access_modifier: "public"
complexity: 1
fan_in: 3
fan_out: 1
tags:
  - method
---
# BookContext::Save
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookContext.cs`


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


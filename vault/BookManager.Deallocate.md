---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookManager.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 3
tags:
  - method
---
# BookManager::Deallocate
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookManager.cs`


#### [[BookManager.Deallocate]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Deallocate(string bookId)
```

**Calls ->**
- [[Log.Debug]]
- [[BookContext.Save]]
- [[ChapterManager.DeallocateAll]]


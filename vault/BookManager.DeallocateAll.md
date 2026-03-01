---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookManager.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 3
tags:
  - method
---
# BookManager::DeallocateAll
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookManager.cs`


#### [[BookManager.DeallocateAll]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void DeallocateAll()
```

**Calls ->**
- [[Log.Debug]]
- [[BookContext.Save]]
- [[ChapterManager.DeallocateAll]]


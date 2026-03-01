---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookManager.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
---
# BookManager::GetOrCreate
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookManager.cs`


#### [[BookManager.GetOrCreate]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private BookContext GetOrCreate(BookDescriptor descriptor)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[BookManager.Load]]
- [[BookManager.Load_2]]


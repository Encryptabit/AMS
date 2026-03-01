---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookManager.cs"
access_modifier: "public"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
---
# BookManager::Load
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookManager.cs`


#### [[BookManager.Load]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public BookContext Load(int index)
```

**Calls ->**
- [[BookManager.GetOrCreate]]

**Called-by <-**
- [[BookManager.TryMoveNext]]
- [[BookManager.TryMovePrevious]]


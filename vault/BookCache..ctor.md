---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookCache.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
---
# BookCache::.ctor
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookCache.cs`


#### [[BookCache..ctor]]
##### What it does:
<member name="M:Ams.Core.Runtime.Book.BookCache.#ctor(System.String)">
    <summary>
    Initializes a new instance of the BookCache class.
    </summary>
    <param name="cacheDirectory">Directory to store cache files. If null, uses default cache directory.</param>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public BookCache(string cacheDirectory = null)
```

**Calls ->**
- [[BookCache.GetDefaultCacheDirectory]]


---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/IBookServices.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
---
# IBookCache::RemoveAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/IBookServices.cs`


#### [[IBookCache.RemoveAsync]]
##### What it does:
<member name="M:Ams.Core.Runtime.Book.IBookCache.RemoveAsync(System.String,System.Threading.CancellationToken)">
    <summary>
    Removes a cached book index for the specified source file.
    </summary>
    <param name="sourceFile">Path to the original source file</param>
    <param name="cancellationToken">Cancellation token</param>
    <returns>True if cache entry was removed</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
Task<bool> RemoveAsync(string sourceFile, CancellationToken cancellationToken = default(CancellationToken))
```


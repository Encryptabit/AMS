---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookCache.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/async
  - llm/data-access
  - llm/error-handling
---
# BookCache::ComputeFileHashAsync
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookCache.cs`

## Summary
**Asynchronously calculates and returns the SHA-256 hash of a file’s contents.**

`ComputeFileHashAsync` computes a SHA-256 hash for a file using asynchronous stream-based IO. It opens a `FileStream` in async mode (`useAsync: true`, shared read), runs `sha256.ComputeHashAsync(stream, cancellationToken)`, and returns the uppercase hex digest via `Convert.ToHexString`. Non-cancellation exceptions are caught and wrapped in `BookCacheException` with file-path context, while cancellation propagates.


#### [[BookCache.ComputeFileHashAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task<string> ComputeFileHashAsync(string filePath, CancellationToken cancellationToken)
```

**Called-by <-**
- [[BookCache.IsValidAsync]]


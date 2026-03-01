---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/error-handling
---
# BookIndexer::ComputeFileHash
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

## Summary
**Calculates a SHA-256 hash for a source file and surfaces failures as `BookIndexException`.**

`ComputeFileHash` synchronously computes a SHA-256 digest of the file at `filePath` by streaming bytes from `File.OpenRead(filePath)` into `sha256.ComputeHash(stream)`. It returns the digest as an uppercase hex string via `Convert.ToHexString(hashBytes)`. Any exception is caught and wrapped in `BookIndexException` with file-path context.


#### [[BookIndexer.ComputeFileHash]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ComputeFileHash(string filePath)
```

**Called-by <-**
- [[BookIndexer.Process]]


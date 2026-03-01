---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/di
  - llm/error-handling
---
# IArtifactResolver::GetBookArtifactFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Return a `FileInfo` for a named artifact under the resolved book root directory.**

`GetBookArtifactFile` is declared on `IArtifactResolver` and implemented in `FileArtifactResolver` as `new(Path.Combine(GetBookRoot(context), fileName))`. It is a pure locator method that returns a `FileInfo` for a caller-specified book-level artifact name and does not perform I/O. Path resolution depends on `GetBookRoot`, which validates `context.Descriptor.RootPath` and throws `InvalidOperationException` when root metadata is missing/blank.


#### [[IArtifactResolver.GetBookArtifactFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
FileInfo GetBookArtifactFile(BookContext context, string fileName)
```

**Called-by <-**
- [[BookContext.ResolveArtifactFile]]


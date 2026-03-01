---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "private"
complexity: 2
fan_in: 3
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
  - llm/data-access
---
# FileArtifactResolver::GetBookRoot
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Validates and returns the absolute filesystem root path for a book context.**

`GetBookRoot` extracts `context.Descriptor.RootPath`, validates it with `string.IsNullOrWhiteSpace`, and throws `InvalidOperationException` when missing. For valid input, it normalizes and absolutizes the path via `Path.GetFullPath(root)` before returning. This centralizes root-path validation/normalization for downstream artifact path builders.


#### [[FileArtifactResolver.GetBookRoot]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string GetBookRoot(BookContext context)
```

**Called-by <-**
- [[FileArtifactResolver.GetBookArtifactFile]]
- [[FileArtifactResolver.LoadPausePolicy]]
- [[FileArtifactResolver.ResolveBookIndexPath]]


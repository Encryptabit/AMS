---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/factory
---
# FileArtifactResolver::GetBookIndexFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Returns a `FileInfo` pointing to the resolved book index artifact location for the given book context.**

`GetBookIndexFile` is a thin path-wrapper that constructs and returns a `FileInfo` for the resolved book-index artifact path. It delegates path computation to `ResolveBookIndexPath(context)` and performs no existence checks or validation itself. The method provides a typed filesystem handle without triggering IO.


#### [[FileArtifactResolver.GetBookIndexFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FileInfo GetBookIndexFile(BookContext context)
```

**Calls ->**
- [[FileArtifactResolver.ResolveBookIndexPath]]


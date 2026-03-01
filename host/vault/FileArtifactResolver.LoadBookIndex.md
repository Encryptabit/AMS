---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/error-handling
---
# FileArtifactResolver::LoadBookIndex
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Loads the book index artifact for a book context from the filesystem when available.**

`LoadBookIndex` resolves the artifact path via `ResolveBookIndexPath(context)` and conditionally loads JSON from disk. If the file exists, it reads the full file (`File.ReadAllText`) and deserializes a `BookIndex` using the resolver’s shared `JsonOptions`; if not, it returns `null`. The method performs no explicit argument validation and lets file/JSON exceptions propagate when present.


#### [[FileArtifactResolver.LoadBookIndex]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public BookIndex LoadBookIndex(BookContext context)
```

**Calls ->**
- [[FileArtifactResolver.ResolveBookIndexPath]]


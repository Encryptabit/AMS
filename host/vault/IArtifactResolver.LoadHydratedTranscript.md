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
  - llm/di
---
# IArtifactResolver::LoadHydratedTranscript
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Declares the resolver API for loading a chapter hydrated transcript artifact.**

`LoadHydratedTranscript` is an interface contract method on `IArtifactResolver`, so it defines behavior without implementation. In source it is declared as `HydratedTranscript? LoadHydratedTranscript(ChapterContext context)`, allowing implementations to return null when no artifact is available. It standardizes chapter-level hydrated transcript retrieval across resolver backends.


#### [[IArtifactResolver.LoadHydratedTranscript]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
HydratedTranscript LoadHydratedTranscript(ChapterContext context)
```

**Called-by <-**
- [[ChapterDocuments..ctor]]


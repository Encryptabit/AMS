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
# IArtifactResolver::SaveHydratedTranscript
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Declares the resolver contract for saving a chapter hydrated transcript artifact.**

`SaveHydratedTranscript` is an `IArtifactResolver` interface member that specifies a write contract without concrete logic. It defines persisting a `HydratedTranscript` for a `ChapterContext`, complementing the corresponding `LoadHydratedTranscript` read operation. Validation, serialization, and storage failure behavior are implementation-defined by concrete resolvers.


#### [[IArtifactResolver.SaveHydratedTranscript]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
void SaveHydratedTranscript(ChapterContext context, HydratedTranscript hydrated)
```

**Called-by <-**
- [[ChapterDocuments..ctor]]


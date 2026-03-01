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
# IArtifactResolver::SaveAnchors
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Declares the resolver API for saving a chapter anchor document artifact.**

`SaveAnchors` is an `IArtifactResolver` interface declaration, so it defines a contract rather than concrete behavior. It specifies persisting an `AnchorDocument` for a given `ChapterContext`, complementing `LoadAnchors` in the same abstraction. Serialization rules, storage location, and error semantics are determined by implementing resolver classes.


#### [[IArtifactResolver.SaveAnchors]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
void SaveAnchors(ChapterContext context, AnchorDocument document)
```

**Called-by <-**
- [[ChapterDocuments..ctor]]


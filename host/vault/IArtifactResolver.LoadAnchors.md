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
# IArtifactResolver::LoadAnchors
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Defines the resolver API for loading a chapter anchor document artifact.**

`LoadAnchors` is an interface-level contract on `IArtifactResolver`, so it provides no executable implementation. In source it is declared as `AnchorDocument? LoadAnchors(ChapterContext context)`, indicating nullable return semantics for absent artifacts. It standardizes chapter anchor retrieval across concrete artifact resolver implementations.


#### [[IArtifactResolver.LoadAnchors]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
AnchorDocument LoadAnchors(ChapterContext context)
```

**Called-by <-**
- [[ChapterDocuments..ctor]]


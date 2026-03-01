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
# IArtifactResolver::LoadAsr
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Defines the resolver API for loading a chapter ASR response artifact.**

`LoadAsr` is an interface contract member on `IArtifactResolver` and has no implementation body. In source it is declared as `AsrResponse? LoadAsr(ChapterContext context)`, so implementations can return null when ASR artifacts are unavailable. It standardizes chapter-level ASR artifact retrieval behind the resolver abstraction.


#### [[IArtifactResolver.LoadAsr]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
AsrResponse LoadAsr(ChapterContext context)
```

**Called-by <-**
- [[ChapterDocuments..ctor]]


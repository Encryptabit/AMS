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
# IArtifactResolver::LoadTranscript
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Defines the artifact resolver contract for loading a chapter transcript from a `ChapterContext`.**

`LoadTranscript` is an interface-level contract on `IArtifactResolver` and contains no executable logic. In source it is declared as `TranscriptIndex? LoadTranscript(ChapterContext context)`, indicating nullable return semantics so implementations can represent missing transcript artifacts. It standardizes chapter transcript retrieval across resolver implementations.


#### [[IArtifactResolver.LoadTranscript]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
TranscriptIndex LoadTranscript(ChapterContext context)
```

**Called-by <-**
- [[ChapterDocuments..ctor]]


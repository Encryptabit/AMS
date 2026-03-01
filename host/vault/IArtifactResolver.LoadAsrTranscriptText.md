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
# IArtifactResolver::LoadAsrTranscriptText
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Declares the resolver API for loading chapter ASR transcript text.**

`LoadAsrTranscriptText` is an `IArtifactResolver` interface method, so it defines a retrieval contract without implementation details. It is declared as `string? LoadAsrTranscriptText(ChapterContext context)`, allowing implementations to represent missing transcript text with a nullable return. The method abstracts chapter-level ASR corpus text access across resolver backends.


#### [[IArtifactResolver.LoadAsrTranscriptText]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
string LoadAsrTranscriptText(ChapterContext context)
```

**Called-by <-**
- [[ChapterDocuments..ctor]]


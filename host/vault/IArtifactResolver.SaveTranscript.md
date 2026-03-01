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
# IArtifactResolver::SaveTranscript
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Declares the resolver API for persisting a chapter transcript artifact.**

`SaveTranscript` is an `IArtifactResolver` interface member, so it declares a persistence contract without implementation details. It defines the write operation for storing a chapter `TranscriptIndex` for a given `ChapterContext`, paired with `LoadTranscript` on the read side. Storage location, validation, and error behavior are delegated to concrete resolver implementations.


#### [[IArtifactResolver.SaveTranscript]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
void SaveTranscript(ChapterContext context, TranscriptIndex transcript)
```

**Called-by <-**
- [[ChapterDocuments..ctor]]


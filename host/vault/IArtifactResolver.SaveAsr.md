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
  - llm/validation
  - llm/di
---
# IArtifactResolver::SaveAsr
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Save a chapter’s ASR response artifact through the artifact resolver, typically as JSON on disk.**

In `IArtifactResolver`, `SaveAsr` is the contract for persisting chapter-level `AsrResponse` data. In `FileArtifactResolver`, it is implemented as `SaveJson(GetChapterArtifactPath(context, "asr.json"), asr)`, which performs a null check, creates the directory if needed, and writes JSON using shared serializer options (camelCase + indented). `ChapterDocuments` binds this in its constructor via `DocumentSlot<AsrResponse>` (`value => resolver.SaveAsr(context, value)`), so ASR persistence flows through the injected resolver.


#### [[IArtifactResolver.SaveAsr]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
void SaveAsr(ChapterContext context, AsrResponse asr)
```

**Called-by <-**
- [[ChapterDocuments..ctor]]


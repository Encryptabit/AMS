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
  - llm/utility
  - llm/di
---
# IArtifactResolver::GetAsrFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Provide the resolved `FileInfo` handle for a chapter’s `asr.json` artifact.**

`GetAsrFile` in `IArtifactResolver` is implemented by `FileArtifactResolver` as `new(GetChapterArtifactPath(context, "asr.json"))`. It is a pure accessor that computes and returns the canonical `FileInfo` for the chapter ASR artifact without performing I/O, and is consumed during constructor setup for `DocumentSlot<AsrResponse>` backing-file metadata.


#### [[IArtifactResolver.GetAsrFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
FileInfo GetAsrFile(ChapterContext context)
```

**Called-by <-**
- [[ChapterDocuments..ctor]]


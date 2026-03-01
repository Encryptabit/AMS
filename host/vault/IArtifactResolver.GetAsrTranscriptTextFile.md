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
# IArtifactResolver::GetAsrTranscriptTextFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Return the resolved `FileInfo` for a chapter’s ASR transcript text artifact file (`asr.corpus.txt`).**

`GetAsrTranscriptTextFile` is an `IArtifactResolver` file-locator method implemented in `FileArtifactResolver` as `new(GetChapterArtifactPath(context, "asr.corpus.txt"))`. It does not access disk content; it only resolves and returns the canonical `FileInfo` for the chapter’s ASR transcript text artifact. This handle is used during constructor-time `DocumentSlot<string>` setup for backing-file metadata.


#### [[IArtifactResolver.GetAsrTranscriptTextFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
FileInfo GetAsrTranscriptTextFile(ChapterContext context)
```

**Called-by <-**
- [[ChapterDocuments..ctor]]


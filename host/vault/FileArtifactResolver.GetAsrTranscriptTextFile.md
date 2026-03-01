---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/factory
---
# FileArtifactResolver::GetAsrTranscriptTextFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Returns a `FileInfo` reference for the chapter’s ASR transcript text file path.**

`GetAsrTranscriptTextFile` is a one-line accessor that constructs and returns a `FileInfo` for the chapter ASR transcript text artifact. It delegates path resolution to `GetChapterArtifactPath(context, "asr.corpus.txt")` and performs no filesystem IO or existence validation. The method provides a typed handle to the canonical text artifact location.


#### [[FileArtifactResolver.GetAsrTranscriptTextFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FileInfo GetAsrTranscriptTextFile(ChapterContext context)
```

**Calls ->**
- [[FileArtifactResolver.GetChapterArtifactPath]]


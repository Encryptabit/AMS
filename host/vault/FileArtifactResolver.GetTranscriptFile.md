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
# FileArtifactResolver::GetTranscriptFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Provides a `FileInfo` reference to the chapter’s transcript artifact file.**

`GetTranscriptFile` is a one-line accessor that returns a new `FileInfo` for the chapter transcript artifact path. It delegates path construction to `GetChapterArtifactPath(context, "align.tx.json")` and performs no filesystem checks or IO. The method simply materializes a typed file handle around the resolved location.


#### [[FileArtifactResolver.GetTranscriptFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FileInfo GetTranscriptFile(ChapterContext context)
```

**Calls ->**
- [[FileArtifactResolver.GetChapterArtifactPath]]


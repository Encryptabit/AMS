---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 2
tags:
  - method
  - llm/data-access
  - llm/utility
---
# FileArtifactResolver::SaveTranscript
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Saves a chapter transcript index artifact to its standard JSON file path.**

`SaveTranscript` is a pass-through persistence method that resolves the chapter transcript artifact path (`GetChapterArtifactPath(context, "align.tx.json")`) and delegates writing to `SaveJson`. It performs no local transformation or explicit null checks, relying on `SaveJson` for validation and serialization behavior. The method synchronously persists the `TranscriptIndex` at the canonical transcript location.


#### [[FileArtifactResolver.SaveTranscript]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void SaveTranscript(ChapterContext context, TranscriptIndex transcript)
```

**Calls ->**
- [[FileArtifactResolver.GetChapterArtifactPath]]
- [[FileArtifactResolver.SaveJson]]


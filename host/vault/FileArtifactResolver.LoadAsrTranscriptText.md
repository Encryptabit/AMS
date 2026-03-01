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
# FileArtifactResolver::LoadAsrTranscriptText
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Loads the raw ASR transcript text artifact for a chapter from its canonical text file path.**

`LoadAsrTranscriptText` resolves the transcript text artifact path (`GetChapterArtifactPath(context, "asr.corpus.txt")`) and delegates file retrieval to `LoadText`. It contains no local parsing, validation, or fallback behavior. Nullability/error semantics are inherited from `LoadText`.


#### [[FileArtifactResolver.LoadAsrTranscriptText]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public string LoadAsrTranscriptText(ChapterContext context)
```

**Calls ->**
- [[FileArtifactResolver.GetChapterArtifactPath]]
- [[FileArtifactResolver.LoadText]]


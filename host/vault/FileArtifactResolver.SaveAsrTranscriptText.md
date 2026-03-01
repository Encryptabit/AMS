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
  - llm/validation
  - llm/error-handling
---
# FileArtifactResolver::SaveAsrTranscriptText
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Validates and saves ASR transcript text for a chapter to its canonical text artifact file.**

`SaveAsrTranscriptText` persists raw transcript text to the chapter artifact folder as `asr.corpus.txt`. It validates `text` with `ArgumentNullException.ThrowIfNull`, resolves the target path via `GetChapterArtifactPath(context, "asr.corpus.txt")`, ensures the directory exists (`EnsureDirectory(path)`), and writes contents synchronously using `File.WriteAllText`. The method overwrites any existing file at that location.


#### [[FileArtifactResolver.SaveAsrTranscriptText]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void SaveAsrTranscriptText(ChapterContext context, string text)
```

**Calls ->**
- [[FileArtifactResolver.EnsureDirectory]]
- [[FileArtifactResolver.GetChapterArtifactPath]]


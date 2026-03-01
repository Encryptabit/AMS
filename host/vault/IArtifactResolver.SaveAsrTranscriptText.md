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
# IArtifactResolver::SaveAsrTranscriptText
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Save the chapter ASR transcript as plain text to its artifact file through the resolver interface.**

In `IArtifactResolver`, `SaveAsrTranscriptText` is the contract for persisting chapter-level ASR transcript text. In `FileArtifactResolver`, it computes `asr.corpus.txt` via `GetChapterArtifactPath(context, "asr.corpus.txt")`, validates `text` with `ArgumentNullException.ThrowIfNull`, ensures the directory exists (`EnsureDirectory`), then writes the content using `File.WriteAllText`. `ChapterDocuments` binds it in its constructor as the `DocumentSlot<string>` save delegate (`value => resolver.SaveAsrTranscriptText(context, value)`) with write-through behavior.


#### [[IArtifactResolver.SaveAsrTranscriptText]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
void SaveAsrTranscriptText(ChapterContext context, string text)
```

**Called-by <-**
- [[ChapterDocuments..ctor]]


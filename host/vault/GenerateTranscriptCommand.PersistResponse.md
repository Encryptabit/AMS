---
namespace: "Ams.Core.Application.Commands"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 2
fan_out: 2
tags:
  - method
  - llm/utility
---
# GenerateTranscriptCommand::PersistResponse
**Path**: `Projects/AMS/host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs`

## Summary
**It stores ASR output onto the chapter document model and derives/logs transcript metadata.**

`PersistResponse` mutates `chapter.Documents` by assigning the raw `AsrResponse` to `Documents.Asr` and generating plain transcript text with `AsrTranscriptBuilder.BuildCorpusText(response)` for `Documents.AsrTranscriptText`. After persistence, it emits a debug log containing `response.ModelVersion` and token count (`response.Tokens.Length`). The method is intentionally side-effect focused and shared by both Nemo and Whisper execution paths.


#### [[GenerateTranscriptCommand.PersistResponse]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void PersistResponse(ChapterContext chapter, AsrResponse response)
```

**Calls ->**
- [[AsrTranscriptBuilder.BuildCorpusText]]
- [[Log.Debug]]

**Called-by <-**
- [[GenerateTranscriptCommand.RunNemoAsync]]
- [[GenerateTranscriptCommand.RunWhisperAsync]]


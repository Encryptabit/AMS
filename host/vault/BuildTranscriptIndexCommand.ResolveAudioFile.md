---
namespace: "Ams.Core.Application.Commands"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Commands/BuildTranscriptIndexCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
---
# BuildTranscriptIndexCommand::ResolveAudioFile
**Path**: `Projects/AMS/host/Ams.Core/Application/Commands/BuildTranscriptIndexCommand.cs`

## Summary
****

`ResolveAudioFile(ChapterContext chapter)` is a private helper used by `ExecuteAsync` to derive and return the chapter’s concrete audio `FileInfo`, encapsulating chapter-to-audio-file resolution in a single static lookup step.


#### [[BuildTranscriptIndexCommand.ResolveAudioFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static FileInfo ResolveAudioFile(ChapterContext chapter)
```

**Called-by <-**
- [[BuildTranscriptIndexCommand.ExecuteAsync]]


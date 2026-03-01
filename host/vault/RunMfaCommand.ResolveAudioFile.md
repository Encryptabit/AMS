---
namespace: "Ams.Core.Application.Commands"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Commands/RunMfaCommand.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# RunMfaCommand::ResolveAudioFile
**Path**: `Projects/AMS/host/Ams.Core/Application/Commands/RunMfaCommand.cs`

## Summary
**It selects the audio input file for MFA by preferring an explicit option and otherwise deriving it from chapter audio buffer metadata.**

`ResolveAudioFile` returns `options.AudioFile` when explicitly provided; otherwise it falls back to the first registered descriptor in `chapter.Descriptor.AudioBuffers`. If no buffer descriptor exists, it throws `InvalidOperationException("No audio buffers are registered for this chapter.")`. For fallback resolution it normalizes the descriptor path via `Path.GetFullPath(...)` and wraps it in `FileInfo`.


#### [[RunMfaCommand.ResolveAudioFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static FileInfo ResolveAudioFile(ChapterContext chapter, RunMfaOptions options)
```

**Called-by <-**
- [[RunMfaCommand.ExecuteAsync]]


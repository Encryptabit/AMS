---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 3
fan_in: 3
fan_out: 0
tags:
  - method
  - llm/validation
  - llm/error-handling
  - llm/utility
---
# AsrProcessor::EnsureModelPath
**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

## Summary
**Enforces that a Whisper model path is provided and points to an existing file before processing.**

`EnsureModelPath` validates ASR model configuration in two steps: it throws `ArgumentException` when `modelPath` is null/whitespace, then throws `FileNotFoundException` when the referenced file does not exist on disk. It performs no normalization or fallback resolution, acting as a strict precondition gate for transcription entry points.


#### [[AsrProcessor.EnsureModelPath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void EnsureModelPath(string modelPath)
```

**Called-by <-**
- [[AsrProcessor.TranscribeBufferAsync]]
- [[AsrProcessor.TranscribeBufferAsync_2]]
- [[AsrProcessor.TranscribeFileAsync]]


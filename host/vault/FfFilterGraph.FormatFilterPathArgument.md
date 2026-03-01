---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# FfFilterGraph::FormatFilterPathArgument
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Formats a normalized filter path into a safely quoted FFmpeg argument fragment.**

`FormatFilterPathArgument` prepares a model path token for embedding in FFmpeg filter syntax. It defaults null/whitespace input to `"rnnoise"`, escapes single quotes inside the value (`'` -> `\\'`), and then wraps the result in escaped single-quote delimiters using the verbatim interpolation `@"\'{escapedQuotes}\'"`. The method is deterministic and side-effect free.


#### [[FfFilterGraph.FormatFilterPathArgument]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string FormatFilterPathArgument(string path)
```

**Called-by <-**
- [[FfFilterGraph.NeuralDenoise]]


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
# FfFilterGraph::NormalizeFilterPathArgument
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Sanitizes and normalizes a filter path argument into a stable slash-delimited token with a safe default.**

`NormalizeFilterPathArgument` canonicalizes a filter model path for downstream FFmpeg argument construction. It substitutes a default `"rnnoise"` token when `path` is null/whitespace, then normalizes separators by converting backslashes to forward slashes and replacing `\r`/`\n` with `/` to keep the value single-line and path-like. The method is pure and performs no I/O or exception handling.


#### [[FfFilterGraph.NormalizeFilterPathArgument]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string NormalizeFilterPathArgument(string path)
```

**Called-by <-**
- [[FfFilterGraph.NeuralDenoise]]


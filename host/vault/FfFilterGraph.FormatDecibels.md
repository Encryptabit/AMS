---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "private"
complexity: 1
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/utility
---
# FfFilterGraph::FormatDecibels
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Formats a numeric value into the project’s standardized decibel string representation for FFmpeg filters.**

`FormatDecibels` is a thin expression-bodied wrapper over `FfUtils.FormatDecibels(value)`. It centralizes dB-string formatting for filter argument construction while introducing no additional branching, validation, or side effects.


#### [[FfFilterGraph.FormatDecibels]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string FormatDecibels(double value)
```

**Calls ->**
- [[FfUtils.FormatDecibels]]

**Called-by <-**
- [[FfFilterGraph.ACompressor]]
- [[FfFilterGraph.ALimiter]]


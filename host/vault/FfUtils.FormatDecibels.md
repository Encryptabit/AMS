---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
---
# FfUtils::FormatDecibels
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs`

## Summary
**Formats a decibel double into a culture-invariant string with a dB suffix for FFmpeg parameter emission.**

FormatDecibels is a one-line utility in FfUtils that returns the interpolated string FormatNumber(value) + "dB". It delegates numeric serialization to FormatNumber, which uses InvariantCulture with the default format "0.####", so output is locale-independent and capped at up to four fractional digits. The method adds no validation, clamping, or error-handling logic.


#### [[FfUtils.FormatDecibels]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static string FormatDecibels(double value)
```

**Calls ->**
- [[FfUtils.FormatNumber]]

**Called-by <-**
- [[FfFilterGraph.FormatDecibels]]


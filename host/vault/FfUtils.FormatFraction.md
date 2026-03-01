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
  - llm/validation
---
# FfUtils::FormatFraction
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs`

## Summary
**Clamp a fraction-like double to a specified range and format it as an invariant-culture string.**

FormatFraction first constrains the input with Math.Clamp(value, min, max), using defaults of 0d and 1d for a normalized fraction range. It then returns FormatNumber(clamped), which formats using the default pattern "0.####" and CultureInfo.InvariantCulture for stable FFmpeg-friendly numeric text.


#### [[FfUtils.FormatFraction]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static string FormatFraction(double value, double min = 0, double max = 1)
```

**Calls ->**
- [[FfUtils.FormatNumber]]

**Called-by <-**
- [[FfFilterGraph.FormatFraction]]


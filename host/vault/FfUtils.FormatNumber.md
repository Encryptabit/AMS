---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs"
access_modifier: "public"
complexity: 1
fan_in: 4
fan_out: 0
tags:
  - method
  - llm/utility
---
# FfUtils::FormatNumber
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs`

## Summary
**Convert a `double` to a deterministic, culture-invariant string for FFmpeg-related output formatting.**

`FfUtils.FormatNumber` is a thin static wrapper around `double.ToString` that formats numeric values using a caller-supplied format string (default `"0.####"`) and `CultureInfo.InvariantCulture`. This guarantees FFmpeg argument strings use `.` as the decimal separator regardless of process locale, with the default pattern emitting up to 4 fractional digits without unnecessary trailing zeros. It is reused by `FormatDecibels`, `FormatDouble`, `FormatFraction`, and `FormatMilliseconds` to centralize numeric string serialization.


#### [[FfUtils.FormatNumber]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static string FormatNumber(double value, string format = "0.####")
```

**Called-by <-**
- [[FfFilterGraph.FormatDouble]]
- [[FfUtils.FormatDecibels]]
- [[FfUtils.FormatFraction]]
- [[FfUtils.FormatMilliseconds]]


---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
---
# FfUtils::FormatMilliseconds
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs`

## Summary
**Provide a millisecond-oriented wrapper that formats a `double` via the shared numeric formatter.**

`FfUtils.FormatMilliseconds(double value)` is an expression-bodied static helper that directly forwards `value` to `FormatNumber(value)` and returns the resulting string. Because `FormatNumber` defaults to `"0.####"` with `CultureInfo.InvariantCulture`, millisecond values are rendered as culture-stable numeric text without localization side effects. The method performs no range checks, clamping, or unit suffixing; it is purely a semantic alias for formatting timing values.


#### [[FfUtils.FormatMilliseconds]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static string FormatMilliseconds(double value)
```

**Calls ->**
- [[FfUtils.FormatNumber]]


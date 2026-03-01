---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# MfaPronunciationProvider::FormatElapsed
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs`

## Summary
**It converts elapsed time into concise log-friendly text with hour-aware formatting.**

`FormatElapsed` formats a duration into a human-readable fixed-width string based on magnitude. It returns `hh:mm:ss` when `elapsed.TotalHours >= 1`; otherwise it returns `mm:ss`, using custom `TimeSpan` format strings for zero-padded output.


#### [[MfaPronunciationProvider.FormatElapsed]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string FormatElapsed(TimeSpan elapsed)
```

**Called-by <-**
- [[MfaPronunciationProvider.RunG2pWithProgressAsync]]


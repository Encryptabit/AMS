---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfLogCapture.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FfLogCapture::Capture
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfLogCapture.cs`

## Summary
**Runs an operation with temporary FFmpeg log redirection and returns the captured log messages.**

`Capture` executes a caller-provided `Action` while temporarily intercepting FFmpeg logs into a per-call `List<string>`. It validates `action`, ensures FFmpeg is initialized (`FfSession.EnsureFiltersAvailable()`), then under a global `lock` installs a custom `av_log` callback, raises log level to `AV_LOG_INFO`, and binds the thread-static collector. In a `finally` block it restores the previous log level, removes the callback, and clears thread-local capture state before returning collected lines.


#### [[FfLogCapture.Capture]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static List<string> Capture(Action action)
```

**Calls ->**
- [[FfSession.EnsureFiltersAvailable]]

**Called-by <-**
- [[FfFilterGraph.CaptureLogs]]


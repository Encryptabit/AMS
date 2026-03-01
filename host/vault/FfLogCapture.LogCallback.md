---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfLogCapture.cs"
access_modifier: "private"
complexity: 3
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# FfLogCapture::LogCallback
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfLogCapture.cs`

## Summary
**Formats FFmpeg log callback data into readable strings and records them in the current capture buffer.**

`LogCallback` is the unmanaged FFmpeg log hook used by `FfLogCapture` to append formatted lines into the active thread-local collector. It exits early if `_current` is null, otherwise formats the incoming log entry with `ffmpeg.av_log_format_line` into a `stackalloc` 1024-byte buffer, converts it via `Marshal.PtrToStringAnsi`, trims whitespace, and appends non-empty lines to the `List<string>`. The method performs no locking itself and relies on the outer capture lifecycle for callback registration scope.


#### [[FfLogCapture.LogCallback]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void LogCallback(void* ptr, int level, string format, byte* vl)
```


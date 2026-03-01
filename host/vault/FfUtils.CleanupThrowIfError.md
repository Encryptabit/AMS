---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs"
access_modifier: "public"
complexity: 5
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# FfUtils::CleanupThrowIfError
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs`

## Summary
**Performs defensive native-resource teardown for encoder setup failures and then raises a terminal exception.**

`CleanupThrowIfError` is a fail-fast cleanup helper that releases partially initialized FFmpeg/GC resources before throwing. It conditionally frees AVIO buffers/context (`av_freep`, `avio_context_free`), tears down codec context state including channel layout (`av_channel_layout_uninit`, `avcodec_free_context`), frees format context (`avformat_free_context`), and releases a pinned `GCHandle` when allocated. After cleanup it throws `InvalidOperationException(message)`.


#### [[FfUtils.CleanupThrowIfError]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void CleanupThrowIfError(string message, AVFormatContext* fmt, AVCodecContext* cc, AVIOContext* avio, GCHandle handle)
```


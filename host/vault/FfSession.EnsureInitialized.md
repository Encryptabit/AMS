---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfSession.cs"
access_modifier: "public"
complexity: 4
fan_in: 6
fan_out: 4
tags:
  - method
  - llm/entry-point
  - llm/validation
  - llm/error-handling
---
# FfSession::EnsureInitialized
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfSession.cs`

## Summary
**Initializes FFmpeg native runtime state exactly once and surfaces actionable errors when native bindings are unavailable.**

`EnsureInitialized` provides one-time, process-wide FFmpeg bootstrap guarded by a double-checked lock on `InitLock` and the `_initialized` flag. Inside the critical section it calls `TrySetRootPath()`, sets FFmpeg log level to `AV_LOG_WARNING`, and initializes networking via `ffmpeg.avformat_network_init()` wrapped by `FfUtils.ThrowIfError`. Binding-related failures (`DllNotFoundException`, `EntryPointNotFoundException`, `NotSupportedException`) are caught through `IsBindingException`, enriched with `BuildFailureHint()`, and rethrown as `InvalidOperationException`.


#### [[FfSession.EnsureInitialized]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfSession.EnsureInitialized">
    <summary>
    Ensures FFmpeg has been initialized for the current process.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void EnsureInitialized()
```

**Calls ->**
- [[FfSession.BuildFailureHint]]
- [[FfSession.IsBindingException]]
- [[FfSession.TrySetRootPath]]
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[FfDecoder.Decode]]
- [[FfDecoder.Probe]]
- [[FfEncoder.Encode]]
- [[StreamingEncoderSink.Initialize]]
- [[FfSession.EnsureFiltersAvailable]]
- [[WavIoTests.FfmpegUnavailable]]


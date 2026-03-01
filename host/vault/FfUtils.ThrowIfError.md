---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs"
access_modifier: "public"
complexity: 2
fan_in: 22
fan_out: 1
tags:
  - method
  - danger/high-fan-in
  - llm/utility
  - llm/error-handling
  - llm/validation
---
# FfUtils::ThrowIfError
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs`

> [!danger] High Fan-In (22)
> This method is called by 22 other methods. Changes here have wide impact.

## Summary
**Converts FFmpeg error codes into consistent .NET exceptions with contextual diagnostics.**

`ThrowIfError` is a centralized FFmpeg return-code guard that treats negative integers as failures and non-negative values as success. It short-circuits when `errorCode >= 0`; otherwise it resolves a human-readable message via `FormatError(errorCode)` and throws `InvalidOperationException` with contextual text including `where` and the numeric code. This method standardizes native-call error propagation across encoder/filter/decoder paths.


#### [[FfUtils.ThrowIfError]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void ThrowIfError(int errorCode, string where)
```

**Calls ->**
- [[FfUtils.FormatError]]

**Called-by <-**
- [[FfDecoder.Decode]]
- [[FfDecoder.Probe]]
- [[FfDecoder.ResampleInto]]
- [[ResampleScratch.Rent]]
- [[FfEncoder.AllocateFrame]]
- [[FfEncoder.DrainEncoder]]
- [[FfEncoder.Encode]]
- [[FfEncoder.EncodeBuffer]]
- [[FfEncoder.EnsureFrameCapacity]]
- [[FfEncoder.FinalizeIo]]
- [[FfEncoder.FlushResampler]]
- [[FfEncoder.SetupIo]]
- [[StreamingEncoderSink.Complete]]
- [[StreamingEncoderSink.Consume]]
- [[StreamingEncoderSink.Initialize]]
- [[FilterGraphExecutor.Drain]]
- [[FilterGraphExecutor.Process]]
- [[FilterGraphExecutor.SendFrame]]
- [[FilterGraphExecutor.SetupSink]]
- [[FilterGraphExecutor.SetupSource]]
- [[FfSession.EnsureInitialized]]
- [[FfUtils.CloneOrDefault]]


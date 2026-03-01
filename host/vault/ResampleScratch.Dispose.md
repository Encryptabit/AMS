---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# ResampleScratch::Dispose
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs`

## Summary
**Disposes the resample scratch object by freeing its unmanaged buffer resources.**

`Dispose()` is a thin resource-cleanup entry point that delegates directly to `Release()`. It follows the standard disposable pattern for this scratch allocator by centralizing unmanaged FFmpeg buffer teardown in one method. No additional state transitions or branching are performed here.


#### [[ResampleScratch.Dispose]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Dispose()
```

**Calls ->**
- [[ResampleScratch.Release]]


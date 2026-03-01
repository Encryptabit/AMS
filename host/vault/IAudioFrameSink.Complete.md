---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/di
  - llm/utility
---
# IAudioFrameSink::Complete
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Defines the sink contract for completing processing after the filter graph has emitted all remaining frames.**

`Complete` on `FfFilterGraphRunner.IAudioFrameSink` is an interface lifecycle method with no local body, representing end-of-stream finalization for sink implementations. `FilterGraphExecutor.Process` invokes it once after all inputs are fed and final drain is performed (`Drain(final: true); _frameSink?.Complete();`). This positions it as the terminal callback for flushing/closing sink-side resources.


#### [[IAudioFrameSink.Complete]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
void Complete()
```

**Called-by <-**
- [[FilterGraphExecutor.Process]]


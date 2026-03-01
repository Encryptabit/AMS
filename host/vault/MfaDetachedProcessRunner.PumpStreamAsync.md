---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaDetachedProcessRunner.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/async
  - llm/utility
---
# MfaDetachedProcessRunner::PumpStreamAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaDetachedProcessRunner.cs`

## Summary
**It asynchronously consumes a stream and records its lines into a synchronized in-memory buffer.**

`PumpStreamAsync` launches a background worker with `Task.Run` that reads `reader` line-by-line using `ReadLineAsync()` until EOF. Each line is appended to `sink` inside `lock (sink)` to avoid concurrent list mutation when multiple pumps run in parallel. It returns the task so callers can await full stream drainage.


#### [[MfaDetachedProcessRunner.PumpStreamAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task PumpStreamAsync(StreamReader reader, List<string> sink)
```

**Called-by <-**
- [[MfaDetachedProcessRunner.RunAsync]]


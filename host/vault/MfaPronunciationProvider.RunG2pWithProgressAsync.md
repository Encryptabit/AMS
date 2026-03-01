---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs"
access_modifier: "private"
complexity: 13
fan_in: 1
fan_out: 5
tags:
  - method
  - llm/async
  - llm/utility
  - llm/error-handling
---
# MfaPronunciationProvider::RunG2pWithProgressAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs`

## Summary
**It executes MFA G2P asynchronously with periodic output-file-based progress reporting and returns both the command result and invocation tag.**

`RunG2pWithProgressAsync` starts a G2P invocation via `_mfaService.GeneratePronunciationsAsync(context, cancellationToken)`, assigns a scoped identifier with `BuildInvocationTag`, and logs start metadata including requested lexeme count and output path. While the task is running, it polls every 5 seconds using `Task.WhenAny(g2pTask, Task.Delay(...))`, inspects output-file existence/size through `GetFileState`, and emits progress logs for file growth, unchanged output, or waiting-for-file states with throttling windows. After completion it awaits the command result, logs final elapsed time (`FormatElapsed`) plus output-file status/size, and returns `(MfaCommandResult, InvocationTag)`.


#### [[MfaPronunciationProvider.RunG2pWithProgressAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<(MfaCommandResult Result, string InvocationTag)> RunG2pWithProgressAsync(MfaChapterContext context, string outputPath, int requestedWordCount, CancellationToken cancellationToken)
```

**Calls ->**
- [[MfaPronunciationProvider.BuildInvocationTag]]
- [[MfaPronunciationProvider.FormatElapsed]]
- [[MfaPronunciationProvider.GetFileState]]
- [[MfaService.GeneratePronunciationsAsync]]
- [[Log.Debug]]

**Called-by <-**
- [[MfaPronunciationProvider.GetPronunciationsAsync]]


---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaInvocationContext.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# MfaInvocationContext::BeginScope
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaInvocationContext.cs`

## Summary
**It establishes a temporary ambient invocation label for the current async context and restores the previous label when the returned scope is disposed.**

`BeginScope` manages an async-flow-local MFA invocation label via `AsyncLocal<string?> CurrentLabel`. It returns a shared no-op disposable when `label` is null/whitespace; otherwise it stores the previous label, sets `CurrentLabel.Value` to `label.Trim()`, and returns a `RestoreScope` disposable that restores the prior value on `Dispose()` (idempotent via `_disposed`). This implements lightweight nested scope semantics without external logging dependencies.


#### [[MfaInvocationContext.BeginScope]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IDisposable BeginScope(string label)
```

**Called-by <-**
- [[PipelineService.RunChapterAsync]]


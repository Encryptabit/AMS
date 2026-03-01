---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Common/Log.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
---
# Log::BeginScope
**Path**: `Projects/AMS/host/Ams.Core/Common/Log.cs`

## Summary
**Starts a logging scope on the current shared logger and returns a disposable handle to end that scope.**

`BeginScope<TState>` is an expression-bodied wrapper over the shared static `logger`, delegating directly to `logger.BeginScope(state)`. The generic constraint `where TState : notnull` enforces a non-null scope state at compile time, and the null-forgiving operator (`!`) forces a non-null `IDisposable` return contract.


#### [[Log.BeginScope]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IDisposable BeginScope<TState>(TState state) where TState : notnull
```


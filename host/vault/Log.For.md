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
  - llm/factory
---
# Log::For
**Path**: `Projects/AMS/host/Ams.Core/Common/Log.cs`

## Summary
**Returns a typed `ILogger` instance for `T` from the shared logging factory.**

`For<T>()` is an expression-bodied helper that delegates directly to the current static `loggerFactory` by calling `CreateLogger<T>()`. It has no branching, validation, or state mutation, so behavior is entirely determined by whatever factory was previously configured in `Log`.


#### [[Log.For]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static ILogger For<T>()
```


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
**Creates and returns a category-based `ILogger` from the currently configured shared logger factory.**

`For(string categoryName)` is an expression-bodied pass-through that calls `loggerFactory.CreateLogger(categoryName)` on the shared static factory. It performs no input checks or fallback handling, so category validation/normalization is deferred to the underlying logging implementation.


#### [[Log.For_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static ILogger For(string categoryName)
```


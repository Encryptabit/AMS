---
namespace: "Ams.Core.Services"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/PipelineService.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
---
# EnvironmentVariableScope::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Services/PipelineService.cs`

## Summary
**Create a disposable scope that temporarily sets an environment variable and enables restoration of its previous value when the scope ends.**

The constructor initializes a scoped environment-variable override by caching the target key in `_name`, reading the current value with `Environment.GetEnvironmentVariable(name)`, and storing it in `_previousValue`. It immediately applies the new value via `Environment.SetEnvironmentVariable(name, value)` and sets `_changed = true` so `Dispose()` will restore the prior state. There is no null/empty validation for `name` or `value`; it delegates all semantics to the underlying `Environment` APIs.


#### [[Ams.Core.Services.PipelineService.EnvironmentVariableScope..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public EnvironmentVariableScope(string name, string value)
```


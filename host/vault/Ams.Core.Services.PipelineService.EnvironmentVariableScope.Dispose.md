---
namespace: "Ams.Core.Services"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/PipelineService.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# EnvironmentVariableScope::Dispose
**Path**: `Projects/AMS/host/Ams.Core/Services/PipelineService.cs`

## Summary
**Reverts a scoped environment-variable override to its previous process-level value at scope cleanup.**

`Dispose()` is the teardown path for `EnvironmentVariableScope`: it checks `_changed` and exits early if no mutation was recorded. When active, it restores the environment variable identified by `_name` back to `_previousValue` using `Environment.SetEnvironmentVariable`, undoing the constructor’s temporary override. In `RunChapterAsync`, it is called from a `finally` block so `MFA_ROOT_DIR` is reset even if MFA processing fails.


#### [[Ams.Core.Services.PipelineService.EnvironmentVariableScope.Dispose]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Dispose()
```

**Called-by <-**
- [[PipelineService.RunChapterAsync]]


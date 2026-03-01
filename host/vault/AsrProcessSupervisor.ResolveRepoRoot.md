---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
---
# AsrProcessSupervisor::ResolveRepoRoot
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`

## Summary
**Locate and cache the repository root at runtime so process startup can build service paths relative to that root.**

`ResolveRepoRoot` memoizes a discovered root path in the static `_repoRoot` field, returning it immediately on subsequent calls. On cache miss, it starts at `AppContext.BaseDirectory` and walks up parent directories up to 8 levels, checking for two sentinel paths (`services/` directory and `ProjectState.md` file) to identify the repo root. If found, it caches and returns that directory; otherwise it returns `null`, which allows `BuildStartInfo` to abort startup path construction.


#### [[AsrProcessSupervisor.ResolveRepoRoot]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ResolveRepoRoot()
```

**Called-by <-**
- [[AsrProcessSupervisor.BuildStartInfo]]


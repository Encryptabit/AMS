---
namespace: "Ams.Cli.Services"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Services/PlugalyzerService.cs"
access_modifier: "internal"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# PlugalyzerService::ResolveExecutable
**Path**: `Projects/AMS/host/Ams.Cli/Services/PlugalyzerService.cs`

## Summary
**Resolves and caches the executable path that `RunAsync` uses to launch Plugalyzer.**

`ResolveExecutable` uses thread-safe lazy initialization (`_cachedExecutable` + double-checked `lock (CacheLock)`) to resolve the Plugalyzer binary path once and reuse it. It first checks `AMS_PLUGALYZER_EXE`, requiring a non-whitespace value that exists on disk, then normalizes it with `Path.GetFullPath`. If the env var path is invalid or missing, it falls back to `ProbeForExecutable()`, and throws `FileNotFoundException` with explicit remediation instructions when probing fails.


#### [[PlugalyzerService.ResolveExecutable]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static string ResolveExecutable()
```

**Calls ->**
- [[PlugalyzerService.ProbeForExecutable]]

**Called-by <-**
- [[PlugalyzerService.RunAsync]]


---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AsrProcessSupervisor::IsLocalBaseUrl
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`

## Summary
**Determine whether a configured base URL points to a loopback/local host endpoint.**

`IsLocalBaseUrl` performs a defensive parse with `Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri)` and immediately returns `false` for invalid or non-absolute inputs. For valid URIs, it classifies the URL as local only when `uri.Host` matches `localhost`, `127.0.0.1`, or `::1` using `StringComparison.OrdinalIgnoreCase`. This gives `EnsureServiceReadyAsync` a strict host-level gate for local-service behavior.


#### [[AsrProcessSupervisor.IsLocalBaseUrl]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool IsLocalBaseUrl(string baseUrl)
```

**Called-by <-**
- [[AsrProcessSupervisor.EnsureServiceReadyAsync]]


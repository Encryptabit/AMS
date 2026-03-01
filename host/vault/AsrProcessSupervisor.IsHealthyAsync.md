---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs"
access_modifier: "private"
complexity: 3
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/async
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AsrProcessSupervisor::IsHealthyAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`

## Summary
**It provides a safe boolean health probe for the configured ASR service URL.**

`IsHealthyAsync` checks ASR endpoint health in a non-throwing way. If Nemo mode is disabled it immediately returns `true`; otherwise it instantiates `AsrClient(baseUrl)` and awaits `client.IsHealthyAsync(cancellationToken)`. Any exception during client creation or probe is caught and converted to `false`.


#### [[AsrProcessSupervisor.IsHealthyAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task<bool> IsHealthyAsync(string baseUrl, CancellationToken cancellationToken)
```

**Calls ->**
- [[AsrClient.IsHealthyAsync]]

**Called-by <-**
- [[AsrProcessSupervisor.EnsureServiceReadyAsync]]
- [[AsrProcessSupervisor.WaitForHealthyAsync]]


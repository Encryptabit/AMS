---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
---
# AsrProcessSupervisor::WaitForHealthyAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`


#### [[AsrProcessSupervisor.WaitForHealthyAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task<bool> WaitForHealthyAsync(string baseUrl, CancellationToken cancellationToken)
```

**Calls ->**
- [[AsrProcessSupervisor.IsHealthyAsync]]

**Called-by <-**
- [[AsrProcessSupervisor.EnsureServiceReadyAsync]]


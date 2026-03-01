---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs"
access_modifier: "private"
complexity: 3
fan_in: 2
fan_out: 1
tags:
  - method
---
# AsrProcessSupervisor::IsHealthyAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`


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


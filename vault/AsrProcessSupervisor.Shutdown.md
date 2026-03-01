---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs"
access_modifier: "internal"
complexity: 7
fan_in: 1
fan_out: 1
tags:
  - method
---
# AsrProcessSupervisor::Shutdown
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`


#### [[AsrProcessSupervisor.Shutdown]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static void Shutdown()
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[AsrProcessSupervisor.RegisterForShutdown]]


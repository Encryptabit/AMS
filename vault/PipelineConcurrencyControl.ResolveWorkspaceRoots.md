---
namespace: "Ams.Core.Application.Pipeline"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Pipeline/PipelineConcurrencyControl.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
---
# PipelineConcurrencyControl::ResolveWorkspaceRoots
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Pipeline/PipelineConcurrencyControl.cs`


#### [[PipelineConcurrencyControl.ResolveWorkspaceRoots]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IEnumerable<string> ResolveWorkspaceRoots(int requestedCount)
```

**Calls ->**
- [[MfaWorkspaceResolver.ResolveWorkspaceRoots]]

**Called-by <-**
- [[PipelineConcurrencyControl..ctor]]


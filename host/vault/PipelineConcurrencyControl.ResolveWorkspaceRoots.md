---
namespace: "Ams.Core.Application.Pipeline"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Pipeline/PipelineConcurrencyControl.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
---
# PipelineConcurrencyControl::ResolveWorkspaceRoots
**Path**: `Projects/AMS/host/Ams.Core/Application/Pipeline/PipelineConcurrencyControl.cs`

## Summary
**It retrieves MFA workspace root candidates for the configured concurrency level by forwarding to the workspace resolver.**

`ResolveWorkspaceRoots` is a private pass-through helper that delegates directly to `MfaWorkspaceResolver.ResolveWorkspaceRoots(requestedCount)`. It exists to encapsulate workspace-root resolution behind the concurrency-control type and is used by the constructor during workspace pool initialization.


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


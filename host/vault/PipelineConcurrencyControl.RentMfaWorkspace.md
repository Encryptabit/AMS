---
namespace: "Ams.Core.Application.Pipeline"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Pipeline/PipelineConcurrencyControl.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# PipelineConcurrencyControl::RentMfaWorkspace
**Path**: `Projects/AMS/host/Ams.Core/Application/Pipeline/PipelineConcurrencyControl.cs`

## Summary
**It returns an available MFA workspace path for the current operation, with deterministic fallback when the queue is exhausted.**

`RentMfaWorkspace` leases an MFA workspace by first attempting a lock-free dequeue from `_mfaWorkspaceQueue` (`TryDequeue`). When the queue is empty, it falls back to `_mfaWorkspaces[0]` if any configured workspace exists; otherwise it yields `null`. This provides best-effort workspace assignment without blocking.


#### [[PipelineConcurrencyControl.RentMfaWorkspace]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public string RentMfaWorkspace()
```

**Called-by <-**
- [[PipelineService.RunChapterAsync]]


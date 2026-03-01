---
namespace: "Ams.Core.Application.Pipeline"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Pipeline/PipelineConcurrencyControl.cs"
access_modifier: "private"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# PipelineConcurrencyControl::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Application/Pipeline/PipelineConcurrencyControl.cs`

## Summary
**It initializes pipeline concurrency limits and preloads the MFA workspace pool used for parallel processing.**

The private constructor normalizes requested concurrency degrees with `Math.Max(1, ...)` and instantiates three bounded semaphores (`BookIndexSemaphore`, `AsrSemaphore`, `MfaSemaphore`) plus `MfaDegree` from the normalized MFA capacity. It resolves MFA workspace roots for that capacity via `ResolveWorkspaceRoots(mfaCapacity)`, stores them in `_mfaWorkspaces`, builds a case-insensitive membership set, and seeds `_mfaWorkspaceQueue` by enqueuing each workspace. This wires both throttling and workspace-rental state in one initialization path.


#### [[PipelineConcurrencyControl..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private PipelineConcurrencyControl(int bookIndexDegree, int asrDegree, int mfaDegree)
```

**Calls ->**
- [[PipelineConcurrencyControl.ResolveWorkspaceRoots]]


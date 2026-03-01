---
namespace: "Ams.Core.Application.Pipeline"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Pipeline/PipelineConcurrencyControl.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/factory
  - llm/utility
  - llm/validation
---
# PipelineConcurrencyControl::CreateShared
**Path**: `Projects/AMS/host/Ams.Core/Application/Pipeline/PipelineConcurrencyControl.cs`

## Summary
**It creates a shared concurrency controller optimized for parallel ASR/MFA work while serializing book indexing.**

`CreateShared` is a static factory that builds a `PipelineConcurrencyControl` with fixed single-threaded book indexing (`bookIndexDegree: 1`) and caller-tuned ASR/MFA limits clamped to at least 1 via `Math.Max(1, ...)`. It delegates construction to the private constructor, which initializes semaphores and workspace pooling.


#### [[PipelineConcurrencyControl.CreateShared]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static PipelineConcurrencyControl CreateShared(int maxAsrParallelism, int maxMfaParallelism)
```

**Called-by <-**
- [[PipelineCommand.RunPipelineForMultipleChaptersAsync]]


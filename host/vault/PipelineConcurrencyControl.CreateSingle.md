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
---
# PipelineConcurrencyControl::CreateSingle
**Path**: `Projects/AMS/host/Ams.Core/Application/Pipeline/PipelineConcurrencyControl.cs`

## Summary
**It creates a concurrency-control instance that allows only one operation at a time for each pipeline stage.**

`CreateSingle` is a static factory that returns a new `PipelineConcurrencyControl` configured for fully serialized execution. It hard-codes all subsystem degrees to `1` (`bookIndexDegree`, `asrDegree`, `mfaDegree`) and delegates to the private constructor.


#### [[PipelineConcurrencyControl.CreateSingle]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static PipelineConcurrencyControl CreateSingle()
```

**Called-by <-**
- [[PipelineCommand.CreateRun]]


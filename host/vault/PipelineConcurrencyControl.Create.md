---
namespace: "Ams.Core.Application.Pipeline"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Pipeline/PipelineConcurrencyControl.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/factory
  - llm/utility
---
# PipelineConcurrencyControl::Create
**Path**: `Projects/AMS/host/Ams.Core/Application/Pipeline/PipelineConcurrencyControl.cs`

## Summary
**It creates a `PipelineConcurrencyControl` from explicit book-index, ASR, and MFA concurrency settings.**

`Create` is a static factory wrapper that returns `new PipelineConcurrencyControl(bookIndexDegree, asrDegree, mfaDegree)`. It performs no local branching or mutation, delegating validation/normalization and resource setup to the constructor.


#### [[PipelineConcurrencyControl.Create]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static PipelineConcurrencyControl Create(int bookIndexDegree, int asrDegree, int mfaDegree)
```


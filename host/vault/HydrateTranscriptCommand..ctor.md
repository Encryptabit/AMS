---
namespace: "Ams.Core.Application.Commands"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Commands/HydrateTranscriptCommand.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/di
  - llm/validation
---
# HydrateTranscriptCommand::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Application/Commands/HydrateTranscriptCommand.cs`

## Summary
**It wires the alignment service dependency into `HydrateTranscriptCommand` with a constructor guard against null injection.**

The constructor injects `IAlignmentService` into the command and stores it in the private readonly `_alignmentService` field for later use by `ExecuteAsync`. It enforces a non-null dependency using null-coalescing throw (`alignmentService ?? throw new ArgumentNullException(nameof(alignmentService))`), making misconfigured DI fail immediately at construction time.


#### [[HydrateTranscriptCommand..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public HydrateTranscriptCommand(IAlignmentService alignmentService)
```


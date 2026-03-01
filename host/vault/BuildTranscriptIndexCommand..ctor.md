---
namespace: "Ams.Core.Application.Commands"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Commands/BuildTranscriptIndexCommand.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 0
tags:
  - method
---
# BuildTranscriptIndexCommand::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Application/Commands/BuildTranscriptIndexCommand.cs`

## Summary
****

`BuildTranscriptIndexCommand(IAlignmentService alignmentService)` is a DI-focused constructor that initializes `BuildTranscriptIndexCommand` with its required `IAlignmentService` dependency, enabling the command’s transcript-index build workflow to delegate alignment operations through that service.


#### [[BuildTranscriptIndexCommand..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public BuildTranscriptIndexCommand(IAlignmentService alignmentService)
```


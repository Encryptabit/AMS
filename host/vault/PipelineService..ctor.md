---
namespace: "Ams.Core.Services"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/PipelineService.cs"
access_modifier: "public"
complexity: 8
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/di
  - llm/validation
  - llm/error-handling
---
# PipelineService::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Services/PipelineService.cs`

## Summary
**Construct `PipelineService` with all pipeline command collaborators and enforce a valid, non-null dependency graph.**

This `PipelineService` constructor injects six required command dependencies and stores them in readonly fields. Each required dependency is guarded with `?? throw new ArgumentNullException(nameof(...))`, so invalid composition fails fast at instantiation. The optional `IPronunciationProvider?` argument is normalized to `NullPronunciationProvider.Instance`, using a Null Object fallback to avoid null checks later in the pipeline.


#### [[PipelineService..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public PipelineService(GenerateTranscriptCommand generateTranscript, ComputeAnchorsCommand computeAnchors, BuildTranscriptIndexCommand buildTranscriptIndex, HydrateTranscriptCommand hydrateTranscript, RunMfaCommand runMfa, MergeTimingsCommand mergeTimings, IPronunciationProvider pronunciationProvider = null)
```


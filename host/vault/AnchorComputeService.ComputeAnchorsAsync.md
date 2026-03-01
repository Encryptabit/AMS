---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/AnchorComputeService.cs"
access_modifier: "public"
complexity: 5
fan_in: 1
fan_out: 8
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/data-access
  - llm/factory
  - llm/di
  - llm/validation
  - llm/error-handling
---
# AnchorComputeService::ComputeAnchorsAsync
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/AnchorComputeService.cs`

## Summary
**Computes and persists chapter anchor mappings between book text and ASR output using configured section and policy options.**

`ComputeAnchorsAsync` orchestrates anchor computation for a chapter by validating `context`, requiring loaded `BookIndex` and ASR (`RequireBookAndAsr`), building preprocessed views/policy, and invoking `AnchorPipeline.ComputeAnchors(...)` with section-detection options and any resolved override section. If the pipeline detects a section, it caches it back to context (`SetDetectedSection`), materializes an `AnchorDocument` via `BuildAnchorDocument`, writes it to `context.Documents.Anchors`, and returns it with `Task.FromResult`. The method is signature-async but executes synchronously and relies on helper methods for invariant enforcement/exception throwing.


#### [[AnchorComputeService.ComputeAnchorsAsync]]
##### What it does:
<member name="M:Ams.Core.Services.Alignment.AnchorComputeService.ComputeAnchorsAsync(Ams.Core.Runtime.Chapter.ChapterContext,Ams.Core.Services.Alignment.AnchorComputationOptions,System.Threading.CancellationToken)">
    <inheritdoc />
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<AnchorDocument> ComputeAnchorsAsync(ChapterContext context, AnchorComputationOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[AnchorPipeline.ComputeAnchors]]
- [[AnchorPreprocessor.BuildAsrView]]
- [[AnchorPreprocessor.BuildBookView]]
- [[ChapterContext.GetOrResolveSection]]
- [[ChapterContext.SetDetectedSection]]
- [[AnchorComputeService.BuildAnchorDocument]]
- [[AnchorComputeService.BuildPolicy]]
- [[AnchorComputeService.RequireBookAndAsr]]

**Called-by <-**
- [[AnchorComputeServiceTests.ComputeAnchorsAsync_NullContext_ThrowsArgumentNullException]]


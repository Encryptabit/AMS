---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/AlignmentService.cs"
access_modifier: "public"
complexity: 5
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/di
  - llm/factory
  - llm/utility
  - llm/validation
---
# AlignmentService::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/AlignmentService.cs`

## Summary
**Initializes alignment facade dependencies, using injected services when provided and sensible defaults otherwise.**

The constructor composes `AlignmentService` by wiring optional dependencies with null-object/default implementations. It resolves a pronunciation provider (`pronunciationProvider ?? NullPronunciationProvider.Instance`), then initializes `_anchorService`, `_transcriptService`, and `_hydrationService` using either injected services or concrete defaults (`new AnchorComputeService()`, `new TranscriptIndexService(provider)`, `new TranscriptHydrationService(provider)`). This provides backward-compatible facade behavior while supporting dependency injection overrides.


#### [[AlignmentService..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public AlignmentService(IPronunciationProvider pronunciationProvider = null, IAnchorComputeService anchorService = null, ITranscriptIndexService transcriptService = null, ITranscriptHydrationService hydrationService = null)
```


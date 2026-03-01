---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/AnchorComputeService.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/factory
  - llm/utility
  - llm/validation
---
# AnchorComputeService::BuildAnchorDocument
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/AnchorComputeService.cs`

## Summary
**Builds an `AnchorDocument` DTO from anchor pipeline output and computation options.**

`BuildAnchorDocument` transforms `AnchorPipelineResult` into a serializable `AnchorDocument` model. It maps pipeline anchors to `AnchorDocumentAnchor` entries with filtered-to-original word-index translation (using `-1` fallback when out of bounds), projects optional windows, and conditionally materializes section metadata. It also records policy settings, token stats, and book window bounds from pipeline/options. The method returns the fully constructed document object.


#### [[AnchorComputeService.BuildAnchorDocument]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static AnchorDocument BuildAnchorDocument(AnchorPipelineResult pipeline, AnchorComputationOptions options)
```

**Called-by <-**
- [[AnchorComputeService.ComputeAnchorsAsync]]


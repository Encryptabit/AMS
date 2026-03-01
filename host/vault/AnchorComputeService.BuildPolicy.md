---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/AnchorComputeService.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/factory
  - llm/validation
---
# AnchorComputeService::BuildPolicy
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/AnchorComputeService.cs`

## Summary
**Builds an `AnchorPolicy` from anchor computation options and stopword configuration.**

`BuildPolicy` maps `AnchorComputationOptions` into a concrete `AnchorPolicy` instance for anchor computation. It selects stopwords based on `UseDomainStopwords` (domain set or empty set), then forwards option values (`NGram`, `TargetPerTokens`, `MinSeparation`, boundary-cross inversion) while hard-coding `AllowDuplicates: false`. This keeps policy construction centralized and deterministic.


#### [[AnchorComputeService.BuildPolicy]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static AnchorPolicy BuildPolicy(AnchorComputationOptions options)
```

**Called-by <-**
- [[AnchorComputeService.ComputeAnchorsAsync]]


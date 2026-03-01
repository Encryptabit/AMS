---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/factory
  - llm/utility
---
# TranscriptIndexService::BuildPolicy
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs`

## Summary
**Builds the concrete anchor-matching policy used by transcript indexing from caller-provided computation options.**

This private static helper materializes an `AnchorPolicy` from `AnchorComputationOptions` by projecting option fields directly into the policy constructor. It conditionally selects the stopword set (`StopwordSets.EnglishPlusDomain` vs an empty ordinal `HashSet<string>`) based on `UseDomainStopwords`, hard-codes `AllowDuplicates: false`, and inverts `AllowBoundaryCross` into `DisallowBoundaryCross`.


#### [[TranscriptIndexService.BuildPolicy]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static AnchorPolicy BuildPolicy(AnchorComputationOptions options)
```

**Called-by <-**
- [[TranscriptIndexService.BuildTranscriptIndexAsync]]


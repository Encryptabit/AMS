---
namespace: "Ams.Cli.Utilities"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Utilities/PausePolicyResolver.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/data-access
---
# PausePolicyResolver::EnumerateCandidates
**Path**: `Projects/AMS/host/Ams.Cli/Utilities/PausePolicyResolver.cs`

## Summary
**Produce the prioritized set of pause-policy candidate paths/names associated with a transcript file for downstream resolution.**

`EnumerateCandidates(FileInfo transcriptFile)` constructs an ordered candidate list of pause-policy path/name strings derived from the transcript file’s location and naming variants. It routes each potential value through `AddCandidate`, which implies centralized normalization/guarding (for example dedupe/null filtering) rather than inline checks. With moderate branching (complexity 6), it provides fallback candidates that `Resolve` can evaluate in priority order.


#### [[PausePolicyResolver.EnumerateCandidates]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IEnumerable<string> EnumerateCandidates(FileInfo transcriptFile)
```

**Calls ->**
- [[PausePolicyResolver.AddCandidate]]

**Called-by <-**
- [[PausePolicyResolver.Resolve]]


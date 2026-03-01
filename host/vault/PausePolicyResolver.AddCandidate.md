---
namespace: "Ams.Cli.Utilities"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Utilities/PausePolicyResolver.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# PausePolicyResolver::AddCandidate
**Path**: `Projects/AMS/host/Ams.Cli/Utilities/PausePolicyResolver.cs`

## Summary
**Adds a candidate path exactly once while maintaining stable insertion order for downstream pause-policy resolution.**

`AddCandidate` is a small deduplication helper invoked by `EnumerateCandidates`: it attempts insertion through `seen` and only appends to `ordered` for first-time paths. Its implementation pattern is a guard plus `HashSet<string>.Add(path)` check followed by `List<string>.Add(path)` when accepted, preserving deterministic discovery order without duplicates. This keeps candidate accumulation efficient (O(1) average per add) and side-effect free beyond the two passed collections.


#### [[PausePolicyResolver.AddCandidate]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void AddCandidate(HashSet<string> seen, List<string> ordered, string path)
```

**Called-by <-**
- [[PausePolicyResolver.EnumerateCandidates]]


---
namespace: "Ams.Core.Processors.Alignment.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/utility
---
# AlignmentResult::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs`

## Summary
**Initializes an `AlignmentResult` with precomputed pair mappings and alignment statistics.**

The `AlignmentResult` constructor is a direct value-assignment initializer for the alignment DTO. It stores the provided `List<Pair>` and metric counts (`matches`, `wildMatches`, `insertions`, `deletions`) into get-only properties with no transformation, validation, or copying. Because `pairs` is assigned by reference, subsequent external mutations of that list would be reflected in the instance.


#### [[Ams.Core.Processors.Alignment.Mfa.AlignmentResult..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public AlignmentResult(List<Pair> pairs, int matches, int wildMatches, int insertions, int deletions)
```


---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "private"
complexity: 27
fan_in: 1
fan_out: 0
tags:
  - method
  - danger/high-complexity
---
# TranscriptAligner::ComputeGuardRangeForMissingSentence
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`

> [!danger] High Complexity (27)
> Cyclomatic complexity: 27. Consider refactoring into smaller methods.


#### [[TranscriptAligner.ComputeGuardRangeForMissingSentence]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static (int? Start, int? End) ComputeGuardRangeForMissingSentence(IReadOnlyList<WordAlign> ops, int sentenceStartWord, int sentenceEndWord)
```

**Called-by <-**
- [[TranscriptAligner.Rollup_2]]


---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "public"
complexity: 16
fan_in: 3
fan_out: 4
tags:
  - method
  - danger/high-complexity
---
# TranscriptAligner::AlignWindows
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`

> [!danger] High Complexity (16)
> Cyclomatic complexity: 16. Consider refactoring into smaller methods.


#### [[TranscriptAligner.AlignWindows]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static List<(int? bi, int? aj, AlignOp op, string reason, double score)> AlignWindows(IReadOnlyList<string> bookNorm, IReadOnlyList<string> asrNorm, IReadOnlyList<(int bLo, int bHi, int aLo, int aHi)> windows, IReadOnlyDictionary<string, string> equiv, ISet<string> fillers, IReadOnlyList<string[]> bookPhonemes = null, IReadOnlyList<string[]> asrPhonemes = null, int maxRun = 8, double maxAvg = 0.6, double phonemeSoftThreshold = 1.01)
```

**Calls ->**
- [[TranscriptAligner.DelCost]]
- [[TranscriptAligner.GetPhonemes]]
- [[TranscriptAligner.InsCost]]
- [[TranscriptAligner.SubCost]]

**Called-by <-**
- [[TranscriptIndexService.BuildWordOperations]]
- [[TxAlignTests.Align_PhonemeMatchTreatsHomophoneAsMatch]]
- [[TxAlignTests.Align_SimpleNearMatch_YieldsSubNotDelIns]]


---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "private"
complexity: 38
fan_in: 1
fan_out: 3
tags:
  - method
  - danger/high-complexity
---
# TranscriptAligner::SynthesizeMissingScriptRanges
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`

> [!danger] High Complexity (38)
> Cyclomatic complexity: 38. Consider refactoring into smaller methods.


#### [[TranscriptAligner.SynthesizeMissingScriptRanges]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void SynthesizeMissingScriptRanges(List<SentenceAlign> sentences, int asrTokenCount, (int? Start, int? End)[] guardRanges)
```

**Calls ->**
- [[TranscriptAligner.FindNextRange]]
- [[TranscriptAligner.FindPreviousRange]]
- [[TranscriptAligner.TryGetConcreteRange]]

**Called-by <-**
- [[TranscriptAligner.Rollup_2]]


---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 3
tags:
  - method
---
# TranscriptAligner::ComputeCer
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`


#### [[TranscriptAligner.ComputeCer]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double ComputeCer(BookIndex book, AsrResponse asr, int bookStart, int bookEnd, int? asrStart, int? asrEnd)
```

**Calls ->**
- [[LevenshteinMetrics.Distance]]
- [[TranscriptAligner.BuildNormalizedWordString]]
- [[TranscriptAligner.BuildNormalizedWordString_2]]

**Called-by <-**
- [[TranscriptAligner.Rollup_2]]


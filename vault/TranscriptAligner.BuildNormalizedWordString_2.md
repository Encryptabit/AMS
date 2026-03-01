---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "private"
complexity: 6
fan_in: 2
fan_out: 1
tags:
  - method
---
# TranscriptAligner::BuildNormalizedWordString
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`


#### [[TranscriptAligner.BuildNormalizedWordString_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string BuildNormalizedWordString(BookIndex book, int start, int end)
```

**Calls ->**
- [[TranscriptAligner.AppendNormalized]]

**Called-by <-**
- [[TranscriptAligner.ComputeCer]]
- [[TranscriptAligner.Rollup_2]]


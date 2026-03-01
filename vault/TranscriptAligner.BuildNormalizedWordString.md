---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "private"
complexity: 7
fan_in: 2
fan_out: 2
tags:
  - method
---
# TranscriptAligner::BuildNormalizedWordString
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`


#### [[TranscriptAligner.BuildNormalizedWordString]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string BuildNormalizedWordString(AsrResponse asr, int? start, int? end)
```

**Calls ->**
- [[AsrResponse.GetWord]]
- [[TranscriptAligner.AppendNormalized]]

**Called-by <-**
- [[TranscriptAligner.ComputeCer]]
- [[TranscriptAligner.Rollup_2]]


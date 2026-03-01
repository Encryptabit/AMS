---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
---
# PhonemeComparer::Similarity
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`


#### [[PhonemeComparer.Similarity]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static double Similarity(string[] a, string[] b)
```

**Calls ->**
- [[LevenshteinMetrics.Similarity]]

**Called-by <-**
- [[TranscriptAligner.HasSoftPhonemeMatch]]


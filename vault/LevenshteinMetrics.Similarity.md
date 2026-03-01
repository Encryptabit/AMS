---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Common/LevenshteinMetrics.cs"
access_modifier: "public"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
---
# LevenshteinMetrics::Similarity
**Path**: `home/cari/repos/AMS/host/Ams.Core/Common/LevenshteinMetrics.cs`


#### [[LevenshteinMetrics.Similarity]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static double Similarity(ReadOnlySpan<string> a, ReadOnlySpan<string> b, StringComparison comparisonType = Ordinal)
```

**Calls ->**
- [[LevenshteinMetrics.Distance_2]]

**Called-by <-**
- [[PhonemeComparer.Similarity]]


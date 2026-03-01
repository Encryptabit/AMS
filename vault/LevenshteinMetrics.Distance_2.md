---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Common/LevenshteinMetrics.cs"
access_modifier: "public"
complexity: 8
fan_in: 1
fan_out: 0
tags:
  - method
---
# LevenshteinMetrics::Distance
**Path**: `home/cari/repos/AMS/host/Ams.Core/Common/LevenshteinMetrics.cs`


#### [[LevenshteinMetrics.Distance_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static int Distance(ReadOnlySpan<string> a, ReadOnlySpan<string> b, StringComparison comparisonType = Ordinal)
```

**Called-by <-**
- [[LevenshteinMetrics.Similarity]]


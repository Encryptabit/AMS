---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Common/TextNormalizer.cs"
access_modifier: "public"
complexity: 4
fan_in: 2
fan_out: 2
tags:
  - method
---
# TextNormalizer::CalculateSimilarity
**Path**: `home/cari/repos/AMS/host/Ams.Core/Common/TextNormalizer.cs`


#### [[TextNormalizer.CalculateSimilarity]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static double CalculateSimilarity(string text1, string text2)
```

**Calls ->**
- [[LevenshteinMetrics.Distance_3]]
- [[TextNormalizer.Normalize]]

**Called-by <-**
- [[ScriptValidator.CalculateMatchCost]]
- [[TextNormalizerTests.CalculateSimilarity_ShouldReturnCorrectSimilarity]]


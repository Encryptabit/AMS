---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# FeatureExtraction::Sigmoid
**Path**: `Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs`

## Summary
**Convert a raw scalar score into a bounded logistic confidence value.**

`Sigmoid(double value)` computes the logistic activation `1.0 / (1.0 + Math.Exp(-value))` in a single expression. It maps unbounded real-valued inputs to a smooth bounded output in `(0, 1)`, making it suitable for turning linear detector scores into probability-like confidences. The method is pure and allocation-free.


#### [[FeatureExtraction.Sigmoid]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double Sigmoid(double value)
```

**Called-by <-**
- [[FeatureExtraction.Detect_2]]


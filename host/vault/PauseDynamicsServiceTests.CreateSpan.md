---
namespace: "Ams.Tests.Prosody"
project: "Ams.Tests"
source_file: "Projects/AMS/host/Ams.Tests/Prosody/PauseDynamicsServiceTests.cs"
access_modifier: "private"
complexity: 1
fan_in: 2
fan_out: 0
tags:
  - method
---
# PauseDynamicsServiceTests::CreateSpan
**Path**: `Projects/AMS/host/Ams.Tests/Prosody/PauseDynamicsServiceTests.cs`


#### [[PauseDynamicsServiceTests.CreateSpan]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static PauseSpan CreateSpan(int leftSentenceId, int rightSentenceId, double startSec, double endSec, PauseClass pauseClass, bool hasGapHint = false)
```

**Called-by <-**
- [[PauseDynamicsServiceTests.PlanTransforms_CompressesSentencePauseOutsideWindow]]
- [[PauseDynamicsServiceTests.PlanTransforms_PreservesTopQuantileForLongestGap]]


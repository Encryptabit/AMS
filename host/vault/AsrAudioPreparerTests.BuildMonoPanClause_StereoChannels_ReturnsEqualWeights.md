---
namespace: "Ams.Tests.Audio"
project: "Ams.Tests"
source_file: "Projects/AMS/host/Ams.Tests/Audio/AsrAudioPreparerTests.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/validation
  - llm/utility
---
# AsrAudioPreparerTests::BuildMonoPanClause_StereoChannels_ReturnsEqualWeights
**Path**: `Projects/AMS/host/Ams.Tests/Audio/AsrAudioPreparerTests.cs`

## Summary
**Verify that BuildMonoPanClause returns a mono pan clause with equal weights for stereo channels.**

In Ams.Tests.Audio.AsrAudioPreparerTests, this unit test is a straight-line check (complexity 1) that makes a single call to BuildMonoPanClause. It validates that stereo channels are folded into mono using equal left/right coefficients, confirming balanced channel weighting in the produced pan clause.


#### [[AsrAudioPreparerTests.BuildMonoPanClause_StereoChannels_ReturnsEqualWeights]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void BuildMonoPanClause_StereoChannels_ReturnsEqualWeights()
```

**Calls ->**
- [[AsrAudioPreparer.BuildMonoPanClause]]


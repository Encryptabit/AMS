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
---
# AsrAudioPreparerTests::BuildMonoPanClause_SurroundChannels_ReturnsCorrectWeights
**Path**: `Projects/AMS/host/Ams.Tests/Audio/AsrAudioPreparerTests.cs`

## Summary
**Validate that surround-channel mono panning produces the correct downmix weights.**

This unit test invokes `BuildMonoPanClause` for a surround-channel scenario and asserts that the returned mono pan clause contains the expected per-channel weight values. With complexity 1, it follows a single straight-line verification path: setup, one method call, and direct expected-vs-actual coefficient validation.


#### [[AsrAudioPreparerTests.BuildMonoPanClause_SurroundChannels_ReturnsCorrectWeights]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void BuildMonoPanClause_SurroundChannels_ReturnsCorrectWeights()
```

**Calls ->**
- [[AsrAudioPreparer.BuildMonoPanClause]]


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
# AsrAudioPreparerTests::BuildMonoPanClause_WeightsAreInvariantCulture
**Path**: `Projects/AMS/host/Ams.Tests/Audio/AsrAudioPreparerTests.cs`

## Summary
**Verify that `BuildMonoPanClause` emits weight values using invariant-culture numeric formatting.**

`BuildMonoPanClause_WeightsAreInvariantCulture` is a focused unit test in `Ams.Tests.Audio.AsrAudioPreparerTests` with cyclomatic complexity 1 that performs a single call to `BuildMonoPanClause`. The test validates that weight serialization in the generated mono pan clause is culture-invariant (e.g., decimal formatting does not depend on current locale). This guards against locale-driven output drift in clause generation.


#### [[AsrAudioPreparerTests.BuildMonoPanClause_WeightsAreInvariantCulture]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void BuildMonoPanClause_WeightsAreInvariantCulture()
```

**Calls ->**
- [[AsrAudioPreparer.BuildMonoPanClause]]


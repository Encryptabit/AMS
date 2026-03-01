---
namespace: "Ams.Tests"
project: "Ams.Tests"
source_file: "Projects/AMS/host/Ams.Tests/AudioProcessorFilterTests.cs"
access_modifier: "private"
complexity: 4
fan_in: 3
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AudioProcessorFilterTests::FiltersUnavailable
**Path**: `Projects/AMS/host/Ams.Tests/AudioProcessorFilterTests.cs`

## Summary
**Determine if required audio-processing filters are unavailable so dependent tests can be conditionally bypassed.**

`FiltersUnavailable()` is a `private static bool` test helper in `Ams.Tests.AudioProcessorFilterTests` that centralizes an environment/capability check and returns `true` when required audio filters are not available. With complexity `4`, it implies multiple conditional branches (and possibly a guarded probe path) collapsed into one gate used by `DetectSilence_FindsInitialGap`, `FadeIn_GraduallyIncreasesAmplitude`, and `Trim_ReturnsExpectedSegment`. This keeps setup/precondition logic out of each test and stabilizes behavior across machines with different filter support.


#### [[AudioProcessorFilterTests.FiltersUnavailable]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool FiltersUnavailable()
```

**Called-by <-**
- [[AudioProcessorFilterTests.DetectSilence_FindsInitialGap]]
- [[AudioProcessorFilterTests.FadeIn_GraduallyIncreasesAmplitude]]
- [[AudioProcessorFilterTests.Trim_ReturnsExpectedSegment]]


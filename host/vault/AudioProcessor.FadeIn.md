---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AudioProcessor.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/data-access
---
# AudioProcessor::FadeIn
**Path**: `Projects/AMS/host/Ams.Core/Processors/AudioProcessor.cs`

## Summary
**Creates a fade-in effect on an audio buffer over the specified duration.**

`FadeIn` applies a leading amplitude ramp by first validating `buffer` and returning the original buffer unchanged when `duration <= TimeSpan.Zero`. For positive durations, it builds an FFmpeg `afade` filter string (`t=in`, `st=0`, `d=<seconds>`) and executes it through `FfFilterGraph.FromBuffer(buffer).Custom(filter).ToBuffer()`. The result is a new buffer with a fade-in envelope from the start.


#### [[AudioProcessor.FadeIn]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AudioBuffer FadeIn(AudioBuffer buffer, TimeSpan duration)
```

**Calls ->**
- [[FfFilterGraph.Custom]]
- [[FfFilterGraph.FromBuffer]]
- [[FfFilterGraph.ToBuffer]]

**Called-by <-**
- [[AudioProcessorFilterTests.FadeIn_GraduallyIncreasesAmplitude]]


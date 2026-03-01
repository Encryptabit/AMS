---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "private"
complexity: 9
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/factory
  - llm/validation
  - llm/error-handling
---
# FilterGraphExecutor::SetupSource
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Creates and initializes the per-input FFmpeg source context and frame buffer required to feed one audio buffer into the filter graph.**

`SetupSource` builds a fully initialized FFmpeg `abuffer` source and reusable input frame for one `AudioBuffer`, then packages them into a `GraphInputState`. It validates `buffer.SampleRate > 0`, resolves/allocates the `abuffer` filter node, constructs init args using `AV_SAMPLE_FMT_FLT` and `buffer.Metadata?.CurrentChannelLayout ?? AudioBufferMetadata.DescribeDefaultLayout(buffer.Channels)`, and initializes the node via `FfUtils.ThrowIfError(avfilter_init_str)`. It then clones a channel layout (`FfUtils.CloneOrDefault`), allocates/configures an `AVFrame` (`format`, `sample_rate`, `nb_samples`, channel layout copy, `av_frame_get_buffer`), and uses a `try/catch` to free native resources before rethrowing on failure.


#### [[FilterGraphExecutor.SetupSource]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private FfFilterGraphRunner.GraphInputState SetupSource(string label, AudioBuffer buffer)
```

**Calls ->**
- [[AudioBufferMetadata.DescribeDefaultLayout]]
- [[FfUtils.CloneOrDefault]]
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[FilterGraphExecutor.CreateInputs]]


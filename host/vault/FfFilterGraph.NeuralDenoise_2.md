---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/factory
---
# FfFilterGraph::NeuralDenoise
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Add an ARNNDN neural denoise filter to the graph using a model-path string input.**

This expression-bodied overload is a convenience wrapper that creates `NeuralDenoiseFilterParams` from a model path and delegates to `NeuralDenoise(NeuralDenoiseFilterParams?)`. It defaults `model` to `"models/sh.rnnn"`, keeping call sites concise while centralizing path resolution/argument formatting and filter emission in the parameter-object overload.


#### [[FfFilterGraph.NeuralDenoise_2]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.NeuralDenoise(System.String)">
    <summary>
    Neural denoiser (libavfilter <c>arnndn</c>).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph NeuralDenoise(string model = "models/sh.rnnn")
```

**Calls ->**
- [[FfFilterGraph.NeuralDenoise]]


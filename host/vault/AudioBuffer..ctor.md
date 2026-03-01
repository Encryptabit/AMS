---
namespace: "Ams.Core.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Artifacts/AudioBuffer.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
---
# AudioBuffer::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Artifacts/AudioBuffer.cs`

## Summary
**Create a new in-memory planar audio buffer with channel/sample metadata and per-channel sample arrays sized to the requested length.**

This constructor initializes an `AudioBuffer` by assigning `Channels`, `SampleRate`, and `Length` from parameters, then setting `Metadata` to the provided value or a synthesized default via `AudioBufferMetadata.CreateDefault(sampleRate, channels)`. It allocates the planar sample storage as a jagged array (`float[channels][]`) and creates one `float[length]` array per channel in a `for` loop. The implementation performs no argument validation, so negative/zero dimensions are left to runtime array allocation behavior.


#### [[AudioBuffer..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public AudioBuffer(int channels, int sampleRate, int length, AudioBufferMetadata metadata = null)
```

**Calls ->**
- [[AudioBufferMetadata.CreateDefault]]


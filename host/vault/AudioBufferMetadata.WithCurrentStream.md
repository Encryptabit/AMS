---
namespace: "Ams.Core.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Artifacts/AudioBufferMetadata.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/factory
---
# AudioBufferMetadata::WithCurrentStream
**Path**: `Projects/AMS/host/Ams.Core/Artifacts/AudioBufferMetadata.cs`

## Summary
**Create a new metadata snapshot with updated current-stream format settings and a fallback channel layout.**

`WithCurrentStream` returns a cloned `AudioBufferMetadata` record using `with`, updating only current-stream fields while preserving existing source/provenance data. It sets `CurrentSampleRate`, `CurrentChannels`, and `CurrentSampleFormat` from arguments, and resolves `CurrentChannelLayout` to the provided value or `DescribeDefaultLayout(channels)` when null. The method is non-mutating and produces a new metadata instance.


#### [[AudioBufferMetadata.WithCurrentStream]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public AudioBufferMetadata WithCurrentStream(int sampleRate, int channels, string sampleFormat, string channelLayout)
```

**Calls ->**
- [[AudioBufferMetadata.DescribeDefaultLayout]]

**Called-by <-**
- [[AudioAccumulator.ToBuffer]]


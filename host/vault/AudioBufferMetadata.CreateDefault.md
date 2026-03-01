---
namespace: "Ams.Core.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Artifacts/AudioBufferMetadata.cs"
access_modifier: "public"
complexity: 1
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/factory
  - llm/utility
---
# AudioBufferMetadata::CreateDefault
**Path**: `Projects/AMS/host/Ams.Core/Artifacts/AudioBufferMetadata.cs`

## Summary
**Create default audio metadata with mirrored source/current stream properties and a derived default channel layout.**

`CreateDefault` builds a baseline `AudioBufferMetadata` record for in-memory buffers from just `sampleRate` and `channels`. It derives a canonical channel layout via `DescribeDefaultLayout(channels)` and assigns both source/current stream fields to the provided values, with both sample formats set to `"fltp"`. The method returns a new immutable-record instance initialized through an object initializer, leaving optional provenance/container fields unset.


#### [[AudioBufferMetadata.CreateDefault]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AudioBufferMetadata CreateDefault(int sampleRate, int channels)
```

**Calls ->**
- [[AudioBufferMetadata.DescribeDefaultLayout]]

**Called-by <-**
- [[AudioBuffer..ctor]]
- [[AudioAccumulator.ToBuffer]]


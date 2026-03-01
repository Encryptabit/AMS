---
namespace: "Ams.Core.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Artifacts/AudioBufferMetadata.cs"
access_modifier: "public"
complexity: 9
fan_in: 6
fan_out: 0
tags:
  - method
  - llm/utility
---
# AudioBufferMetadata::DescribeDefaultLayout
**Path**: `Projects/AMS/host/Ams.Core/Artifacts/AudioBufferMetadata.cs`

## Summary
**Provide a default human-readable channel layout string for a given channel count.**

`DescribeDefaultLayout` maps channel counts to canonical layout labels via a C# switch expression. It returns named layouts for common configurations (`1 -> mono`, `2 -> stereo`, `3 -> 2.1`, `4 -> quad`, `5 -> 5.0`, `6 -> 5.1`, `7 -> 6.1`, `8 -> 7.1`) and falls back to an interpolated `"{channels}c"` token for nonstandard counts. The method centralizes layout defaults used across decode/setup/buffer conversion paths.


#### [[AudioBufferMetadata.DescribeDefaultLayout]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static string DescribeDefaultLayout(int channels)
```

**Called-by <-**
- [[AudioBufferMetadata.CreateDefault]]
- [[AudioBufferMetadata.WithCurrentStream]]
- [[FfDecoder.Decode]]
- [[AudioAccumulator.ToBuffer]]
- [[FilterGraphExecutor.ConfigureChannelLayouts]]
- [[FilterGraphExecutor.SetupSource]]


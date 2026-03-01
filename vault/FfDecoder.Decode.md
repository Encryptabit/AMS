---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs"
access_modifier: "public"
complexity: 56
fan_in: 1
fan_out: 11
tags:
  - method
  - danger/high-complexity
---
# FfDecoder::Decode
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs`

> [!danger] High Complexity (56)
> Cyclomatic complexity: 56. Consider refactoring into smaller methods.


#### [[FfDecoder.Decode]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AudioBuffer Decode(string path, AudioDecodeOptions options)
```

**Calls ->**
- [[AudioBuffer.UpdateMetadata]]
- [[AudioBufferMetadata.DescribeDefaultLayout]]
- [[FfDecoder.AppendSamples]]
- [[FfPacket.Unref]]
- [[FfDecoder.GetSampleFormatName]]
- [[FfDecoder.PtrToStringUtf8]]
- [[FfDecoder.ReadTags]]
- [[FfSession.EnsureInitialized]]
- [[FfUtils.CloneOrDefault]]
- [[FfUtils.CreateDefaultChannelLayout]]
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[AudioProcessor.Decode]]


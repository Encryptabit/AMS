---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs"
access_modifier: "public"
complexity: 19
fan_in: 1
fan_out: 4
tags:
  - method
  - danger/high-complexity
  - llm/data-access
  - llm/validation
  - llm/error-handling
  - llm/utility
---
# FfDecoder::Probe
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs`

> [!danger] High Complexity (19)
> Cyclomatic complexity: 19. Consider refactoring into smaller methods.

## Summary
**Reads structural audio metadata from a media file and returns it as an `AudioInfo` object.**

This method probes an audio file via FFmpeg to produce `AudioInfo` metadata without decoding full samples. It validates file existence, initializes FFmpeg, opens the input and stream info with `ThrowIfError`, finds the best audio stream, and extracts container/sample rate/channel count with fallbacks (including channel default `1`). Duration is derived from stream time base or container duration, bit depth is resolved from raw/coded sample bits or inferred from `AVSampleFormat`, and the format context is always closed in `finally`.


#### [[FfDecoder.Probe]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AudioInfo Probe(string path)
```

**Calls ->**
- [[FfDecoder.PtrToStringUtf8]]
- [[FfDecoder.ReadTags]]
- [[FfSession.EnsureInitialized]]
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[AudioProcessor.Probe]]


---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# FfDecoder::GetSampleFormatName
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs`

## Summary
**Converts an FFmpeg sample-format enum into an optional display name for audio metadata.**

This helper resolves a human-readable FFmpeg sample-format name from an `AVSampleFormat` enum value. It calls `ffmpeg.av_get_sample_fmt_name(format)` and normalizes blank/whitespace results to `null` (via `string.IsNullOrWhiteSpace`). The method provides a safe metadata string for decode diagnostics without throwing on unknown formats.


#### [[FfDecoder.GetSampleFormatName]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string GetSampleFormatName(AVSampleFormat format)
```

**Called-by <-**
- [[FfDecoder.Decode]]


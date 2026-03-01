---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs"
access_modifier: "private"
complexity: 6
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
---
# FfDecoder::ReadTags
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs`

## Summary
**Reads native FFmpeg tag metadata and converts it into a managed read-only string dictionary.**

This unsafe helper extracts FFmpeg metadata key/value pairs from an `AVDictionary*` into a managed dictionary. It returns `null` when the pointer is null, otherwise iterates entries with `av_dict_get(..., AV_DICT_IGNORE_SUFFIX)`, converts UTF-8 keys/values via `Marshal.PtrToStringUTF8`, and stores non-empty keys using case-insensitive lookup (`StringComparer.OrdinalIgnoreCase`). It returns `null` again if no valid tags were collected.


#### [[FfDecoder.ReadTags]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyDictionary<string, string> ReadTags(AVDictionary* metadata)
```

**Called-by <-**
- [[FfDecoder.Decode]]
- [[FfDecoder.Probe]]


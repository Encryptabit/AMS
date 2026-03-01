---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/error-handling
  - llm/validation
---
# FfFrame::Dispose
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs`

## Summary
**Releases the native FFmpeg frame allocation held by `FfFrame`.**

`Dispose()` frees the unmanaged `AVFrame` owned by this wrapper and prevents reuse after cleanup. It checks `_pointer` for null, passes a local copy into `av_frame_free(&local)`, and then sets `_pointer` to `null` to make repeated disposal safe. This provides deterministic native resource release for decode loops.


#### [[FfFrame.Dispose]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Dispose()
```


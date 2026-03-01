---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs"
access_modifier: "public"
complexity: 3
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FfUtils::CloneOrDefault
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs`

## Summary
**Produces a safe channel-layout value by preferring a cloned input layout and falling back to a default derived from channel count.**

`CloneOrDefault` returns a usable `AVChannelLayout` by cloning an existing layout when available or generating a default layout from channel count otherwise. If `layout` is non-null and `layout->nb_channels > 0`, it copies into a local `result` via `av_channel_layout_copy` guarded by `ThrowIfError`; on success it returns the clone. When no valid input layout exists, it calls `av_channel_layout_default(&result, fallbackChannels)` and returns that fallback value.


#### [[FfUtils.CloneOrDefault]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AVChannelLayout CloneOrDefault(AVChannelLayout* layout, int fallbackChannels)
```

**Calls ->**
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[FfDecoder.Decode]]
- [[FilterGraphExecutor.SetupSource]]


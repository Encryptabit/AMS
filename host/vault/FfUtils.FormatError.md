---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs"
access_modifier: "public"
complexity: 2
fan_in: 4
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# FfUtils::FormatError
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs`

## Summary
**Formats an FFmpeg error code into a human-readable diagnostic string.**

`FormatError` converts an FFmpeg numeric error code into a readable message using `av_strerror`. It allocates a fixed unmanaged buffer (`stackalloc byte[1024]`), writes the native error string into it, and marshals the result via `Marshal.PtrToStringAnsi`. If marshaling yields null, it falls back to `errorCode.ToString(CultureInfo.InvariantCulture)`.


#### [[FfUtils.FormatError]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static string FormatError(int errorCode)
```

**Called-by <-**
- [[FilterGraphExecutor.ConfigureChannelLayouts]]
- [[FilterGraphExecutor.ConfigureGraph]]
- [[FilterGraphExecutor.ConfigureIntOption]]
- [[FfUtils.ThrowIfError]]


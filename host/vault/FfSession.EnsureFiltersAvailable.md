---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfSession.cs"
access_modifier: "public"
complexity: 2
fan_in: 2
fan_out: 2
tags:
  - method
  - llm/validation
  - llm/error-handling
---
# FfSession::EnsureFiltersAvailable
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfSession.cs`

## Summary
**Ensures the process has FFmpeg initialized and confirms `libavfilter` support is present before running filter-graph features.**

`EnsureFiltersAvailable` is a guard method that verifies FFmpeg filter-graph capability before filter-dependent operations proceed. It first calls `EnsureInitialized()`, then `EnsureFilterProbe()`, and inspects the cached `_filtersAvailable` flag. When filtering support is absent, it throws `NotSupportedException` with explicit deployment guidance about `libavfilter` and expected `ExtTools/ffmpeg/*` locations.


#### [[FfSession.EnsureFiltersAvailable]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void EnsureFiltersAvailable()
```

**Calls ->**
- [[FfSession.EnsureFilterProbe]]
- [[FfSession.EnsureInitialized]]

**Called-by <-**
- [[FilterGraphExecutor..ctor]]
- [[FfLogCapture.Capture]]


---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfSession.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/validation
  - llm/error-handling
---
# FfSession::EnsureFilterProbe
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfSession.cs`

## Summary
**Caches whether FFmpeg filter-graph APIs are available in the current runtime by running a one-time native probe.**

`EnsureFilterProbe` lazily performs a one-time capability check for `libavfilter`, guarded by `_filtersChecked` with double-checked locking on `InitLock`. Inside the probe it attempts `ffmpeg.avfilter_graph_alloc()` and sets `_filtersAvailable` based on allocation success, explicitly treating `EntryPointNotFoundException` and `NotSupportedException` as unsupported-filter signals (`false`). A `finally` block always frees any allocated graph via `avfilter_graph_free` and marks `_filtersChecked = true` so probing is not repeated.


#### [[FfSession.EnsureFilterProbe]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void EnsureFilterProbe()
```

**Called-by <-**
- [[FfSession.EnsureFiltersAvailable]]


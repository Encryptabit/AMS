---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/SpliceBoundaryService.cs"
access_modifier: "private"
complexity: 10
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# SpliceBoundaryService::RefineBoundary
**Path**: `Projects/AMS/host/Ams.Core/Audio/SpliceBoundaryService.cs`

## Summary
**Refine one splice boundary by preferring nearest silence-center placement and otherwise using energy-edge snapping with guarded fallbacks.**

`RefineBoundary` first clamps `searchLeftSec/searchRightSec` to `[0, bufferDurationSec]` and returns `(roughSec, Original)` when the window is invalid. It trims that window via `AudioProcessor.Trim`, runs `AudioProcessor.DetectSilence` with `opts.SilenceThresholdDb`/`opts.MinSilenceDuration`, then selects the silence interval whose center is nearest to `roughSec` (converted to region-relative time), returning its absolute center with `BoundaryMethod.SilenceCenter`. If no usable silence is found, it falls back to `AudioProcessor.SnapToEnergy` seeded at `roughSec`, picks `snapped.StartSec` or `snapped.EndSec` based on `isStartBoundary`, applies directional `opts.EnergyBackoffSec`, and only accepts it when delta from rough exceeds `5ms` (then clamps to search bounds) as `BoundaryMethod.SnapEnergy`. Otherwise it returns the original rough boundary with `BoundaryMethod.Original`.


#### [[SpliceBoundaryService.RefineBoundary]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static (double position, BoundaryMethod method) RefineBoundary(AudioBuffer buffer, double bufferDurationSec, double roughSec, double searchLeftSec, double searchRightSec, bool isStartBoundary, SpliceBoundaryOptions opts)
```

**Calls ->**
- [[AudioProcessor.DetectSilence]]
- [[AudioProcessor.SnapToEnergy]]
- [[AudioProcessor.Trim]]

**Called-by <-**
- [[SpliceBoundaryService.RefineBoundaries]]


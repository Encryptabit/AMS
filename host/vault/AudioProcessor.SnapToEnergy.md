---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AudioProcessor.Analysis.cs"
access_modifier: "public"
complexity: 23
fan_in: 1
fan_out: 7
tags:
  - method
  - danger/high-complexity
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AudioProcessor::SnapToEnergy
**Path**: `Projects/AMS/host/Ams.Core/Processors/AudioProcessor.Analysis.cs`

> [!danger] High Complexity (23)
> Cyclomatic complexity: 23. Consider refactoring into smaller methods.

## Summary
**Snaps approximate speech boundaries to nearby energy transitions in an audio buffer using RMS thresholding and hangover/tail-guard heuristics.**

`SnapToEnergy` refines a seed `TimingRange` against waveform energy by converting all timing knobs (window/step/search/preroll/postroll/hangover) from ms/sec to sample counts, translating dB thresholds via `DbToLinear`, and clamping seed bounds with `ClampToBuffer`. It determines a speech start by scanning backward/forward with windowed RMS (`RmsForward`/`RmsBackward`/`WindowRms`) against enter/exit thresholds, including a fallback `ResolveStart` pass when the initial search stalls. It then resolves the end using hangover-aware forward tracking of `lastSpeech`, applies an additional tail guard extension when recent frames indicate sustained energy, clamps to valid bounds, and finally returns pre/post-roll-adjusted times in seconds.


#### [[AudioProcessor.SnapToEnergy]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static TimingRange SnapToEnergy(AudioBuffer buffer, TimingRange seed, double enterThresholdDb = -45, double exitThresholdDb = -57, double searchWindowSec = 0.8, double windowMs = 25, double stepMs = 5, double preRollMs = 10, double postRollMs = 10, double hangoverMs = 90)
```

**Calls ->**
- [[AudioProcessor.CalculateRms]]
- [[AudioProcessor.ClampToBuffer]]
- [[AudioProcessor.DbToLinear]]
- [[ResolveStart]]
- [[RmsBackward]]
- [[RmsForward]]
- [[WindowRms]]

**Called-by <-**
- [[SpliceBoundaryService.RefineBoundary]]


---
namespace: "Ams.Workstation.Server.Controllers"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Controllers/AudioController.cs"
access_modifier: "public"
complexity: 16
fan_in: 0
fan_out: 1
tags:
  - method
  - danger/high-complexity
---
# AudioController::GetWaveformData
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Controllers/AudioController.cs`

> [!danger] High Complexity (16)
> Cyclomatic complexity: 16. Consider refactoring into smaller methods.


#### [[AudioController.GetWaveformData]]
##### What it does:
<member name="M:Ams.Workstation.Server.Controllers.AudioController.GetWaveformData(System.String,System.Nullable{System.Double},System.Nullable{System.Double},System.Int32)">
    <summary>
    Returns normalized RMS amplitude data for a segment of an audio file.
    Used by mini waveform thumbnails to render lightweight canvas-based visualizations
    without requiring a full wavesurfer.js instance.
    </summary>
    <param name="path">Absolute path to the audio file.</param>
    <param name="start">Optional start time in seconds.</param>
    <param name="end">Optional end time in seconds.</param>
    <param name="points">Number of amplitude data points to return (clamped to 20-500).</param>
    <returns>JSON array of normalized floats (0.0 to 1.0).</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IActionResult GetWaveformData(string path, double? start = null, double? end = null, int points = 100)
```

**Calls ->**
- [[AudioProcessor.Decode]]


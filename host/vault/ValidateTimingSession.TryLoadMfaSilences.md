---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# ValidateTimingSession::TryLoadMfaSilences
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Attempts to load valid MFA silence time spans for a transcript from candidate TextGrid files.**

`TryLoadMfaSilences` enumerates TextGrid path candidates from `BuildTextGridCandidates(transcript)`, skips non-existent files, and attempts to parse each with `TextGridParser.ParseWordIntervals(path)`. It derives silence spans by filtering intervals with `IsSilenceLabel(interval.Text)`, projecting `(interval.Start, interval.End)`, and discarding zero/negative durations (`End - Start > 0.0`). The method short-circuits on the first non-empty silence list, suppresses parse/load failures with a blanket `catch` to continue scanning, and returns `null` when no usable silences are found.


#### [[ValidateTimingSession.TryLoadMfaSilences]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private IReadOnlyList<(double Start, double End)> TryLoadMfaSilences(TranscriptIndex transcript)
```

**Calls ->**
- [[ValidateTimingSession.BuildTextGridCandidates]]
- [[ValidateTimingSession.IsSilenceLabel]]
- [[TextGridParser.ParseWordIntervals]]

**Called-by <-**
- [[ValidateTimingSession.LoadSessionContextAsync]]


---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AudioProcessor.cs"
access_modifier: "public"
complexity: 13
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# SilenceLogParser::Parse
**Path**: `Projects/AMS/host/Ams.Core/Processors/AudioProcessor.cs`

## Summary
**Parses silencedetect diagnostic logs into structured silence time ranges with resilient start/end/duration reconstruction.**

`Parse` converts FFmpeg silencedetect log lines into `SilenceInterval` records by scanning each line with a compiled regex that captures `silence_start`, `silence_end`, and `silence_duration` values. It maintains parser state (`currentStart`, `lastDuration`) across lines, supporting cases where start/duration/end arrive in separate log entries. On each detected end, it derives duration and start using available fields (current line duration, previous duration, or `end - currentStart` fallback), clamps negative values to zero, appends an interval, and resets state. The method returns all parsed intervals in encounter order.


#### [[SilenceLogParser.Parse]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IReadOnlyList<SilenceInterval> Parse(IEnumerable<string> logs)
```

**Called-by <-**
- [[AudioProcessor.DetectSilence]]


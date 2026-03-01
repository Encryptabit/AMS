---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# PauseDynamicsService::ExtractTimesFromScript
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`

## Summary
**Converts punctuation positions in script text into approximate timeline timestamps inside a sentence.**

`ExtractTimesFromScript` estimates punctuation anchor times within a sentence by scanning `scriptText` for characters where `IsIntraSentencePunctuation` is true. It returns an empty list when text is null/whitespace or `duration <= 0`, otherwise maps each punctuation character index to a timestamp using proportional interpolation across the sentence span (`sentenceStart + (idx / max(1, textLength-1)) * duration`). When at least one punctuation mark is found, it logs a debug message including source label, count, and sentence ID.


#### [[PauseDynamicsService.ExtractTimesFromScript]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<double> ExtractTimesFromScript(int sentenceId, double sentenceStart, double duration, string scriptText, string sourceLabel)
```

**Calls ->**
- [[Log.Debug]]
- [[PauseDynamicsService.IsIntraSentencePunctuation]]

**Called-by <-**
- [[PauseDynamicsService.GetPunctuationTimes]]


---
namespace: "Ams.Core.Pipeline"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Pipeline/SentenceRefinementService.cs"
access_modifier: "public"
complexity: 23
fan_in: 1
fan_out: 4
tags:
  - method
  - danger/high-complexity
---
# SentenceRefinementService::RefineAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Pipeline/SentenceRefinementService.cs`

> [!danger] High Complexity (23)
> Cyclomatic complexity: 23. Consider refactoring into smaller methods.


#### [[SentenceRefinementService.RefineAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<IReadOnlyList<SentenceRefined>> RefineAsync(string audioPath, TranscriptIndex tx, AsrResponse asr, string language = "eng", bool useSilence = true, double silenceThresholdDb = -30, double silenceMinDurationSec = 0.1)
```

**Calls ->**
- [[SentenceTiming.WithEnd]]
- [[AsrResponse.GetWord]]
- [[SentenceRefinementService.DetectSilencesAsync]]
- [[SentenceRefinementService.RunAeneasAsync]]

**Called-by <-**
- [[RefineSentencesCommand.RunAsync]]


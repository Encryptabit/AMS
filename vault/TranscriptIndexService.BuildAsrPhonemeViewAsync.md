---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 3
tags:
  - method
---
# TranscriptIndexService::BuildAsrPhonemeViewAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs`


#### [[TranscriptIndexService.BuildAsrPhonemeViewAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<string[][]> BuildAsrPhonemeViewAsync(AsrResponse asr, AsrAnchorView asrView, CancellationToken cancellationToken)
```

**Calls ->**
- [[AsrResponse.GetWord]]
- [[IPronunciationProvider.GetPronunciationsAsync]]
- [[PronunciationHelper.NormalizeForLookup]]

**Called-by <-**
- [[TranscriptIndexService.BuildTranscriptIndexAsync]]


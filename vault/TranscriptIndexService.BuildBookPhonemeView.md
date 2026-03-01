---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
---
# TranscriptIndexService::BuildBookPhonemeView
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs`


#### [[TranscriptIndexService.BuildBookPhonemeView]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string[][] BuildBookPhonemeView(BookIndex book, IReadOnlyList<int> filteredToOriginal, int filteredCount)
```

**Called-by <-**
- [[TranscriptIndexService.BuildTranscriptIndexAsync]]


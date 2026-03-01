---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
---
# TranscriptHydrationService::JoinBook
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs`


#### [[TranscriptHydrationService.JoinBook]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string JoinBook(BookIndex book, int start, int end)
```

**Calls ->**
- [[TranscriptHydrationService.NormalizeSurface]]

**Called-by <-**
- [[TranscriptHydrationService.BuildHydratedTranscriptAsync]]


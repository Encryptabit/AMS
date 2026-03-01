---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs"
access_modifier: "private"
complexity: 13
fan_in: 1
fan_out: 5
tags:
  - method
---
# MfaPronunciationProvider::RunG2pWithProgressAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs`


#### [[MfaPronunciationProvider.RunG2pWithProgressAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<(MfaCommandResult Result, string InvocationTag)> RunG2pWithProgressAsync(MfaChapterContext context, string outputPath, int requestedWordCount, CancellationToken cancellationToken)
```

**Calls ->**
- [[MfaPronunciationProvider.BuildInvocationTag]]
- [[MfaPronunciationProvider.FormatElapsed]]
- [[MfaPronunciationProvider.GetFileState]]
- [[MfaService.GeneratePronunciationsAsync]]
- [[Log.Debug]]

**Called-by <-**
- [[MfaPronunciationProvider.GetPronunciationsAsync]]


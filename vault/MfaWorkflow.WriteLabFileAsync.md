---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 2
tags:
  - method
---
# MfaWorkflow::WriteLabFileAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs`


#### [[MfaWorkflow.WriteLabFileAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task WriteLabFileAsync(FileInfo hydrateFile, ChapterContext chapterContext, string labPath, FileInfo corpusSource, CancellationToken cancellationToken)
```

**Calls ->**
- [[MfaWorkflow.PrepareLabLines]]
- [[Log.Debug]]

**Called-by <-**
- [[MfaWorkflow.RunChapterAsync]]


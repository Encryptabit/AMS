---
namespace: "Ams.Core.Application.Commands"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Commands/MergeTimingsCommand.cs"
access_modifier: "public"
complexity: 42
fan_in: 1
fan_out: 6
tags:
  - method
  - danger/high-complexity
---
# MergeTimingsCommand::ExecuteAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Commands/MergeTimingsCommand.cs`

> [!danger] High Complexity (42)
> Cyclomatic complexity: 42. Consider refactoring into smaller methods.


#### [[MergeTimingsCommand.ExecuteAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task ExecuteAsync(ChapterContext chapter, MergeTimingsOptions options, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[MergeTimingsCommand.ResolveChapterWordWindow]]
- [[Log.Debug]]
- [[MfaTimingMerger.MergeAndApply]]
- [[TextGridParser.ParseWordIntervals]]
- [[ChapterDocuments.GetTextGridFile]]
- [[ChapterDocuments.SaveChanges]]

**Called-by <-**
- [[PipelineService.RunChapterAsync]]


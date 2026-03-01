---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 0
tags:
  - method
---
# ChapterManager::ResolveChapterRoot
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`


#### [[ChapterManager.ResolveChapterRoot]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ResolveChapterRoot(DirectoryInfo chapterDirectory, FileInfo audioFile, FileInfo asrFile, DirectoryInfo bookIndexDirectory, string chapterStem)
```

**Called-by <-**
- [[ChapterManager.CreateContext]]


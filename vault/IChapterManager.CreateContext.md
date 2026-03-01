---
namespace: "Ams.Core.Runtime.Interfaces"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Interfaces/IChapterManager.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
---
# IChapterManager::CreateContext
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Interfaces/IChapterManager.cs`


#### [[IChapterManager.CreateContext]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
ChapterContextHandle CreateContext(FileInfo bookIndexFile, FileInfo asrFile = null, FileInfo transcriptFile = null, FileInfo hydrateFile = null, FileInfo audioFile = null, DirectoryInfo chapterDirectory = null, string chapterId = null, bool reloadBookIndex = false)
```


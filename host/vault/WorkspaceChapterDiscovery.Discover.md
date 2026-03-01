---
namespace: "Ams.Core.Runtime.Workspace"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Workspace/WorkspaceChapterDiscovery.cs"
access_modifier: "public"
complexity: 4
fan_in: 0
fan_out: 2
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/factory
  - llm/validation
  - llm/error-handling
---
# WorkspaceChapterDiscovery::Discover
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Workspace/WorkspaceChapterDiscovery.cs`

## Summary
**Discovers chapter folders under a workspace root and materializes `ChapterDescriptor` objects with inferred audio buffer paths.**

`Discover` builds chapter descriptors from the filesystem by scanning immediate subdirectories under `bookRoot`. It validates `bookRoot`, throws `DirectoryNotFoundException` when the root does not exist, then for each chapter directory computes buffer paths via local helpers (`RawPath`, `RootPath`), preferring chapter-local `{chapterId}.wav` and falling back to book-root raw audio. It creates `AudioBufferDescriptor` entries for `raw`, `treated`, and `filtered`, then emits a `ChapterDescriptor(chapterId, dir.FullName, buffers)` for each directory.


#### [[WorkspaceChapterDiscovery.Discover]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IReadOnlyList<ChapterDescriptor> Discover(string bookRoot)
```

**Calls ->**
- [[RawPath]]
- [[RootPath]]


---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/factory
  - llm/validation
---
# ChapterManager::CloneWithAliases
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

## Summary
**Constructs a merged chapter descriptor that keeps existing identity while combining aliases and selected incoming metadata.**

`CloneWithAliases` creates a merged descriptor that preserves identity while combining alias and metadata inputs. It builds aliases via `MergeAliases(existing, incoming)`, prefers non-blank incoming `RootPath`, prefers existing audio buffers when available (otherwise incoming), prefers existing documents when present (otherwise incoming), and fills book word bounds from incoming with fallback to existing. It returns a new `ChapterDescriptor` anchored to `existing.ChapterId`.


#### [[ChapterManager.CloneWithAliases]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static ChapterDescriptor CloneWithAliases(ChapterDescriptor existing, ChapterDescriptor incoming)
```

**Calls ->**
- [[ChapterManager.MergeAliases]]

**Called-by <-**
- [[ChapterManager.EnsureChapterDescriptor]]


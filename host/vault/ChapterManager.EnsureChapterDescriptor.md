---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 7
tags:
  - method
  - llm/utility
  - llm/factory
  - llm/validation
---
# ChapterManager::EnsureChapterDescriptor
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

## Summary
**Ensures a chapter descriptor is deduplicated and merged against existing descriptors before being inserted or updated.**

`EnsureChapterDescriptor` resolves a canonical descriptor before persistence by trying several match strategies against existing chapter metadata. It first checks direct ID presence (`Contains(template.ChapterId)`), then normalized-alias matching (`NormalizeChapterId` + `FindByAlias`), root-path matching (`TryMatchByRootPath`), and root-slug matching (`TryMatchByRootSlug`); for matched descriptors it merges aliases/details via `CloneWithAliases(...)` before upserting. All paths end in `UpsertDescriptor(...)`, either with a merged descriptor or the original template.


#### [[ChapterManager.EnsureChapterDescriptor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private ChapterDescriptor EnsureChapterDescriptor(ChapterDescriptor template)
```

**Calls ->**
- [[ChapterManager.CloneWithAliases]]
- [[ChapterManager.Contains]]
- [[ChapterManager.FindByAlias]]
- [[ChapterManager.NormalizeChapterId]]
- [[ChapterManager.TryMatchByRootPath]]
- [[ChapterManager.TryMatchByRootSlug]]
- [[ChapterManager.UpsertDescriptor]]

**Called-by <-**
- [[ChapterManager.CreateContext]]


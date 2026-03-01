---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "Ams.Core.Runtime.Interfaces.IChapterManager"
member_count: 31
dependency_count: 1
pattern: ~
tags:
  - class
---

# ChapterManager

> Class in `Ams.Core.Runtime.Chapter`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

**Implements**:
- [[IChapterManager]]

## Dependencies
- [[BookContext]] (`bookContext`)

## Properties
- `Count`: int
- `Descriptors`: IReadOnlyList<ChapterDescriptor>
- `Current`: ChapterContext
- `MaxCachedContexts`: int
- `DefaultMaxCachedContexts`: int
- `JsonOptions`: JsonSerializerOptions
- `_bookContext`: BookContext
- `_descriptors`: List<ChapterDescriptor>
- `_cache`: Dictionary<string, ChapterContext>
- `_usageNodes`: Dictionary<string, LinkedListNode<string>>
- `_usageOrder`: LinkedList<string>
- `_maxCachedContexts`: int
- `_cursor`: int

## Members
- [[ChapterManager..ctor]]
- [[ChapterManager.Load]]
- [[ChapterManager.Load_2]]
- [[ChapterManager.Contains]]
- [[ChapterManager.CreateContext]]
- [[ChapterManager.UpsertDescriptor]]
- [[ChapterManager.TryMoveNext]]
- [[ChapterManager.TryMovePrevious]]
- [[ChapterManager.Reset]]
- [[ChapterManager.Deallocate]]
- [[ChapterManager.DeallocateAll]]
- [[ChapterManager.GetOrCreate]]
- [[ChapterManager.TrackUsage]]
- [[ChapterManager.RemoveUsageNode]]
- [[ChapterManager.EnsureCapacity]]
- [[ChapterManager.MergeDescriptors]]
- [[ChapterManager.EnsureChapterDescriptor]]
- [[ChapterManager.FindByAlias]]
- [[ChapterManager.TryMatchByRootPath]]
- [[ChapterManager.TryMatchByRootSlug]]
- [[ChapterManager.CloneWithAliases]]
- [[ChapterManager.MergeAliases]]
- [[ChapterManager.BuildAliasSet]]
- [[ChapterManager.TryResolveSection]]
- [[ChapterManager.TryResolveSectionFromAliases]]
- [[ChapterManager.AddAlias]]
- [[ChapterManager.NormalizePath]]
- [[ChapterManager.NormalizeChapterId]]
- [[ChapterManager.DetermineChapterStem]]
- [[ChapterManager.ResolveChapterRoot]]
- [[ChapterManager.LoadJson]]


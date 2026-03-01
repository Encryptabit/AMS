---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/UndoService.cs"
access_modifier: "public"
base_class: ~
interfaces: []
member_count: 14
dependency_count: 1
pattern: "service"
tags:
  - class
  - pattern/service
---

# UndoService

> Class in `Ams.Workstation.Server.Services`

**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/UndoService.cs`

## Dependencies
- [[BlazorWorkspace]] (`workspace`)

## Properties
- `JsonOptions`: JsonSerializerOptions
- `_workspace`: BlazorWorkspace
- `_lock`: object
- `_records`: Dictionary<string, List<UndoRecord>>
- `_loadedChapters`: HashSet<string>

## Members
- [[UndoService..ctor]]
- [[UndoService.SaveOriginalSegment]]
- [[UndoService.GetUndoRecords]]
- [[UndoService.GetUndoRecord]]
- [[UndoService.LoadOriginalSegment]]
- [[UndoService.RemoveRecord]]
- [[UndoService.HasUndo]]
- [[UndoService.GetWorkDir]]
- [[UndoService.GetChapterUndoDir]]
- [[UndoService.GetManifestPath]]
- [[UndoService.GetNextVersion]]
- [[UndoService.GetUndoRecordInternal]]
- [[UndoService.EnsureLoaded]]
- [[UndoService.SaveManifest]]


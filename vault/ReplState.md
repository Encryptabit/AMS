---
namespace: "Ams.Cli.Repl"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Repl/ReplContext.cs"
access_modifier: "internal"
base_class: ~
interfaces: []
member_count: 18
dependency_count: 0
pattern: ~
tags:
  - class
---

# ReplState

> Class in `Ams.Cli.Repl`

**Path**: `home/cari/repos/AMS/host/Ams.Cli/Repl/ReplContext.cs`

## Properties
- `WorkingDirectory`: string
- `Chapters`: List<FileInfo>
- `RunAllChapters`: bool
- `SelectedChapterIndex`: int?
- `SelectedChapter`: FileInfo?
- `ActiveChapter`: FileInfo?
- `ActiveChapterStem`: string?
- `WorkingDirectoryLabel`: string
- `ScopeLabel`: string
- `Workspace`: IWorkspace
- `_chapterOverride`: AsyncLocal<FileInfo?>
- `_stateFilePath`: string
- `_suppressPersist`: bool
- `_pendingChapterName`: string?
- `_pendingRunAll`: bool
- `_lastSelectedChapterName`: string?
- `_workspace`: CliWorkspace?

## Members
- [[ReplState..ctor]]
- [[ReplState.SetWorkingDirectory]]
- [[ReplState.RefreshChapters]]
- [[ReplState.ListChapters]]
- [[ReplState.PrintState]]
- [[ReplState.UseAllChapters]]
- [[ReplState.UseChapterByIndex]]
- [[ReplState.UseChapterByName]]
- [[ReplState.BeginChapterScope]]
- [[ReplState.ClearChapterScope]]
- [[ReplState.ResolveChapterFile]]
- [[ReplState.ResolveBookIndex]]
- [[ReplState.InitializeFallbackSelection]]
- [[ReplState.SelectChapterByNameInternal]]
- [[ReplState.SelectChapterByIndexInternal]]
- [[ReplState.LoadPersistedState]]
- [[ReplState.PersistState]]
- [[ReplState.ResolveStateFilePath]]


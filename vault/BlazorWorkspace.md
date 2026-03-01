---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/BlazorWorkspace.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "Ams.Core.Runtime.Workspace.IWorkspace"
  - "System.IDisposable"
member_count: 14
dependency_count: 0
pattern: ~
tags:
  - class
---

# BlazorWorkspace

> Class in `Ams.Workstation.Server.Services`

**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/BlazorWorkspace.cs`

**Implements**:
- [[IWorkspace]]
- IDisposable

## Properties
- `RootPath`: string
- `Book`: BookContext
- `IsInitialized`: bool
- `WorkingDirectory`: string?
- `AvailableChapters`: List<string>
- `HasBookIndex`: bool
- `CurrentChapterName`: string?
- `CurrentChapterHandle`: ChapterContextHandle?
- `CachedBookOverview`: BookOverview?
- `StateFilePath`: string
- `_manager`: BookManager?
- `_currentChapterHandle`: ChapterContextHandle?
- `_rootPath`: string?
- `_disposed`: bool
- `_stemByTitle`: Dictionary<string, string>
- `_chapterHandles`: Dictionary<string, ChapterContextHandle>

## Members
- [[BlazorWorkspace..ctor]]
- [[BlazorWorkspace.OpenChapter]]
- [[BlazorWorkspace.TryGetHydratedTranscript]]
- [[BlazorWorkspace.SetWorkingDirectory]]
- [[BlazorWorkspace.SelectChapter]]
- [[BlazorWorkspace.Clear]]
- [[BlazorWorkspace.SetCachedBookOverview]]
- [[BlazorWorkspace.GetStemForChapter]]
- [[BlazorWorkspace.ResolveDefaultBookIndex]]
- [[BlazorWorkspace.BuildDescriptor]]
- [[BlazorWorkspace.LoadChaptersFromIndex]]
- [[BlazorWorkspace.LoadPersistedState]]
- [[BlazorWorkspace.SavePersistedState]]
- [[BlazorWorkspace.Dispose]]


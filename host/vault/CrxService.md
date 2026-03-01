---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/CrxService.cs"
access_modifier: "public"
base_class: ~
interfaces: []
member_count: 27
dependency_count: 2
pattern: "service"
tags:
  - class
  - pattern/service
---

# CrxService

> Class in `Ams.Workstation.Server.Services`

**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/CrxService.cs`

## Dependencies
- [[BlazorWorkspace]] (`workspace`)
- [[AudioExportService]] (`audioExportService`)

## Properties
- `CrxTemplatePath`: string
- `CrxDataRowStart`: int
- `JsonOptions`: JsonSerializerOptions
- `_workspace`: BlazorWorkspace
- `_audioExportService`: AudioExportService

## Members
- [[CrxService..ctor]]
- [[CrxService.Submit]]
- [[CrxService.GetEntries]]
- [[CrxService.TryReadExcelEntries]]
- [[CrxService.TryReadExcelEntriesOpenXml]]
- [[CrxService.ReadSharedStrings]]
- [[CrxService.ExtractColumnIndex]]
- [[CrxService.GetCellText]]
- [[CrxService.TryReadJsonEntries]]
- [[CrxService.EnsureJsonSeededFromExcel]]
- [[CrxService.BuildSeededLegacyEntry]]
- [[CrxService.ResolveAudioFileForError]]
- [[CrxService.ResolveWorkspaceChapterName]]
- [[CrxService.ChapterMatches]]
- [[CrxService.TryExtractChapterNumber]]
- [[CrxService.NormalizeForCompare]]
- [[CrxService.TryParseTimecode]]
- [[CrxService.DistanceToSentenceCenter]]
- [[CrxService.AppendOrUpdateJsonEntry]]
- [[CrxService.TryRemoveJsonEntry]]
- [[CrxService.WriteJsonEntries]]
- [[CrxService.AppendCrxEntry]]
- [[CrxService.EnsureExcelReady]]
- [[CrxService.TryDeleteExportedFile]]
- [[CrxService.GetCrxExcelPath]]
- [[CrxService.GetCrxJsonPath]]
- [[CrxService.FormatTimecode]]


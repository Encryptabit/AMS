---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDocuments.cs"
access_modifier: "public"
base_class: ~
interfaces: []
member_count: 11
dependency_count: 2
pattern: ~
tags:
  - class
---

# ChapterDocuments

> Class in `Ams.Core.Runtime.Chapter`

**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDocuments.cs`

## Dependencies
- [[ChapterContext]] (`context`)
- [[IArtifactResolver]] (`resolver`)

## Properties
- `Transcript`: TranscriptIndex?
- `HydratedTranscript`: HydratedTranscript?
- `Anchors`: AnchorDocument?
- `Asr`: AsrResponse?
- `AsrTranscriptText`: string?
- `PauseAdjustments`: PauseAdjustmentsDocument?
- `PausePolicy`: PausePolicy
- `TextGrid`: TextGridDocument?
- `IsDirty`: bool
- `_transcript`: DocumentSlot<TranscriptIndex>
- `_hydratedTranscript`: DocumentSlot<HydratedTranscript>
- `_anchors`: DocumentSlot<AnchorDocument>
- `_asr`: DocumentSlot<AsrResponse>
- `_asrTranscriptText`: DocumentSlot<string>
- `_pauseAdjustments`: DocumentSlot<PauseAdjustmentsDocument>
- `_pausePolicy`: DocumentSlot<PausePolicy>
- `_textGrid`: DocumentSlot<TextGridDocument>

## Members
- [[ChapterDocuments..ctor]]
- [[ChapterDocuments.SaveChanges]]
- [[ChapterDocuments.InvalidateTextGrid]]
- [[ChapterDocuments.GetTranscriptFile]]
- [[ChapterDocuments.GetHydratedTranscriptFile]]
- [[ChapterDocuments.GetAnchorsFile]]
- [[ChapterDocuments.GetAsrFile]]
- [[ChapterDocuments.GetAsrTranscriptTextFile]]
- [[ChapterDocuments.GetPauseAdjustmentsFile]]
- [[ChapterDocuments.GetPausePolicyFile]]
- [[ChapterDocuments.GetTextGridFile]]


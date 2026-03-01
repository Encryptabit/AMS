---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Cli.Commands.ValidateTimingSession.SessionContext>"
member_count: 1
dependency_count: 5
pattern: ~
tags:
  - class
---

# SessionContext

> Record in `Ams.Cli.Commands`

**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

**Implements**:
- IEquatable

## Dependencies
- [[TranscriptIndex]] (`Transcript`)
- [[BookIndex]] (`BookIndex`)
- [[HydratedTranscript]] (`Hydrated`)
- [[PauseAnalysisReport]] (`Analysis`)
- [[ChapterPauseMap]] (`PauseMap`)

## Properties
- `Transcript`: TranscriptIndex
- `BookIndex`: BookIndex
- `Hydrated`: HydratedTranscript
- `SentenceLookup`: IReadOnlyDictionary<int, string>
- `Paragraphs`: IReadOnlyList<ParagraphInfo>
- `SentenceToParagraph`: IReadOnlyDictionary<int, int>
- `ParagraphSentences`: IReadOnlyDictionary<int, IReadOnlyList<int>>
- `Analysis`: PauseAnalysisReport
- `PauseMap`: ChapterPauseMap

## Members


---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "Ams.Core.Services.Alignment.ITranscriptHydrationService"
member_count: 15
dependency_count: 1
pattern: "service"
tags:
  - class
  - pattern/service
---

# TranscriptHydrationService

> Class in `Ams.Core.Services.Alignment`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs`

**Implements**:
- [[ITranscriptHydrationService]]

## Dependencies
- [[Ams.Core.Runtime.Book.IPronunciationProvider_]] (`pronunciationProvider`)

## Properties
- `_pronunciationProvider`: IPronunciationProvider
- `EmptyTokenPhonemeView`: TokenPhonemeView

## Members
- [[TranscriptHydrationService..ctor]]
- [[TranscriptHydrationService.HydrateTranscriptAsync]]
- [[TranscriptHydrationService.BuildHydratedTranscriptAsync]]
- [[TranscriptHydrationService.BuildPhonemeAwareScoringOptions]]
- [[TranscriptHydrationService.BuildPronunciationFallbackAsync]]
- [[TranscriptHydrationService.BuildBookScoringView]]
- [[TranscriptHydrationService.BuildAsrScoringView]]
- [[TranscriptHydrationService.ResolveBookWordPhonemes]]
- [[TranscriptHydrationService.BuildParagraphScoringView]]
- [[TranscriptHydrationService.NormalizeSurface]]
- [[TranscriptHydrationService.JoinBook]]
- [[TranscriptHydrationService.JoinAsr]]
- [[TranscriptHydrationService.ResolveSentenceStatus]]
- [[TranscriptHydrationService.ResolveParagraphStatus]]
- [[TranscriptHydrationService.BuildParagraphScript]]


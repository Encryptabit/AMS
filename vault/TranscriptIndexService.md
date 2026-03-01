---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "Ams.Core.Services.Alignment.ITranscriptIndexService"
member_count: 13
dependency_count: 1
pattern: "service"
tags:
  - class
  - pattern/service
---

# TranscriptIndexService

> Class in `Ams.Core.Services.Alignment`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs`

**Implements**:
- [[ITranscriptIndexService]]

## Dependencies
- [[Ams.Core.Runtime.Book.IPronunciationProvider_]] (`pronunciationProvider`)

## Properties
- `_pronunciationProvider`: IPronunciationProvider
- `Logger`: ILogger

## Members
- [[TranscriptIndexService..ctor]]
- [[TranscriptIndexService.BuildTranscriptIndexAsync]]
- [[TranscriptIndexService.RequireBookAndAsr]]
- [[TranscriptIndexService.BuildPolicy]]
- [[TranscriptIndexService.BuildAnchorDocument]]
- [[TranscriptIndexService.BuildWordOperations]]
- [[TranscriptIndexService.BuildRollups]]
- [[TranscriptIndexService.BuildBookPhonemeView]]
- [[TranscriptIndexService.BuildAsrPhonemeViewAsync]]
- [[TranscriptIndexService.BuildFallbackWindows]]
- [[TranscriptIndexService.ComputeTiming]]
- [[TranscriptIndexService.ResolveDefaultAudioPath]]
- [[TranscriptIndexService.ResolveDefaultBookIndexPath]]


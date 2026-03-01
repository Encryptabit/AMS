---
namespace: "Ams.Core.Application.Pipeline"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Pipeline/PipelineRunOptions.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Application.Pipeline.PipelineRunOptions>"
member_count: 0
dependency_count: 0
pattern: ~
tags:
  - class
---

# PipelineRunOptions

> Record in `Ams.Core.Application.Pipeline`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Pipeline/PipelineRunOptions.cs`

**Implements**:
- IEquatable

## Properties
- `BookFile`: FileInfo
- `BookIndexFile`: FileInfo
- `AudioFile`: FileInfo
- `ChapterDirectory`: DirectoryInfo?
- `ChapterId`: string
- `Force`: bool
- `ForceIndex`: bool
- `StartStage`: PipelineStage
- `EndStage`: PipelineStage
- `AverageWordsPerMinute`: double
- `TranscriptOptions`: GenerateTranscriptOptions?
- `AnchorOptions`: AnchorComputationOptions?
- `TranscriptIndexOptions`: BuildTranscriptIndexOptions?
- `HydrationOptions`: HydrationOptions?
- `MfaOptions`: RunMfaOptions?
- `MergeOptions`: MergeTimingsOptions?
- `SkipTreatedCopy`: bool
- `TreatedCopyFile`: FileInfo?
- `Concurrency`: PipelineConcurrencyControl?


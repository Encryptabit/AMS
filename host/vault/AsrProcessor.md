---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "public"
base_class: ~
interfaces: []
member_count: 23
dependency_count: 0
pattern: ~
tags:
  - class
---

# AsrProcessor

> Class in `Ams.Core.Processors`

**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

## Properties
- `DtwFallbackMinAudioSeconds`: double
- `DtwFallbackCoverageThreshold`: double
- `DtwFallbackMinimumShortfallSeconds`: double
- `_whisperInflight`: int

## Members
- [[AsrProcessor.TranscribeFileAsync]]
- [[AsrProcessor.TranscribeBufferAsync_2]]
- [[AsrProcessor.TranscribeBufferAsync]]
- [[AsrProcessor.TranscribeBufferInternalAsync]]
- [[AsrProcessor.DetectLanguageInternalAsync]]
- [[AsrProcessor.TranscribeWithWhisperNetAsync]]
- [[AsrProcessor.RunWhisperPassAsync]]
- [[AsrProcessor.ShouldRetryWithoutDtw]]
- [[AsrProcessor.IsDtwEffectivelyEnabled]]
- [[AsrProcessor.ComputeAudioDurationSeconds]]
- [[AsrProcessor.ComputeTranscriptEndSeconds]]
- [[AsrProcessor.EnsureModelPath]]
- [[AsrProcessor.CreateFactoryOptions]]
- [[AsrProcessor.ResolveDtwPreset]]
- [[AsrProcessor.ConfigureBuilder]]
- [[AsrProcessor.ConfigureBuilder_2]]
- [[AsrProcessor.AppendTokens]]
- [[AsrProcessor.AggregateTokens_2]]
- [[AsrProcessor.AggregateTokens]]
- [[AsrProcessor.IsSpecialToken]]
- [[AsrProcessor.NormalizeTokenText]]
- [[AsrProcessor.HasExplicitWordBoundary]]
- [[AsrProcessor.ExtractMonoSamples]]


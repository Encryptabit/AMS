---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
base_class: ~
interfaces: []
member_count: 59
dependency_count: 1
pattern: ~
tags:
  - class
---

# FfFilterGraph

> Class in `Ams.Core.Services.Integrations.FFmpeg`

**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Dependencies
- [[AudioBuffer]] (`buffer`)

## Properties
- `_inputs`: List<GraphInput>
- `_clauses`: List<string>
- `_inputLabel`: string
- `_outputLabel`: string
- `_customGraphOverride`: bool
- `_formatPinned`: bool

## Members
- [[FfFilterGraph..ctor]]
- [[FfFilterGraph.FromBuffer]]
- [[FfFilterGraph.WithInput]]
- [[FfFilterGraph.UseInput]]
- [[FfFilterGraph.WithOutputLabel]]
- [[FfFilterGraph.AFormat]]
- [[FfFilterGraph.HighPass_2]]
- [[FfFilterGraph.HighPass]]
- [[FfFilterGraph.LowPass_2]]
- [[FfFilterGraph.LowPass]]
- [[FfFilterGraph.DeEsser_2]]
- [[FfFilterGraph.DeEsser]]
- [[FfFilterGraph.FftDenoise_2]]
- [[FfFilterGraph.FftDenoise]]
- [[FfFilterGraph.NeuralDenoise_2]]
- [[FfFilterGraph.NeuralDenoise]]
- [[FfFilterGraph.ACompressor_2]]
- [[FfFilterGraph.ACompressor]]
- [[FfFilterGraph.ALimiter_2]]
- [[FfFilterGraph.ALimiter]]
- [[FfFilterGraph.LoudNorm_2]]
- [[FfFilterGraph.LoudNorm]]
- [[FfFilterGraph.DynaudNorm]]
- [[FfFilterGraph.Resample]]
- [[FfFilterGraph.SilenceRemove_2]]
- [[FfFilterGraph.SilenceRemove]]
- [[FfFilterGraph.AStats_2]]
- [[FfFilterGraph.AStats]]
- [[FfFilterGraph.ASetNSamples]]
- [[FfFilterGraph.AShowInfo]]
- [[FfFilterGraph.AspectralStats]]
- [[FfFilterGraph.EbuR128_2]]
- [[FfFilterGraph.EbuR128]]
- [[FfFilterGraph.Custom]]
- [[FfFilterGraph.UseCustomGraph]]
- [[FfFilterGraph.BuildSpec]]
- [[FfFilterGraph.ToBuffer]]
- [[FfFilterGraph.RunDiscardingOutput]]
- [[FfFilterGraph.CaptureLogs]]
- [[FfFilterGraph.Measure]]
- [[FfFilterGraph.StreamToWave]]
- [[FfFilterGraph.Gain_2]]
- [[FfFilterGraph.Gain]]
- [[FfFilterGraph.BuildInputs]]
- [[FfFilterGraph.AddFilter]]
- [[FfFilterGraph.AddFilter_2]]
- [[FfFilterGraph.AddRawFilter]]
- [[FfFilterGraph.AddInput]]
- [[FfFilterGraph.SerializeArguments]]
- [[FfFilterGraph.Escape]]
- [[FfFilterGraph.ResolveFilterAssetPath]]
- [[FfFilterGraph.NormalizeFilterPathArgument]]
- [[FfFilterGraph.FormatFilterPathArgument]]
- [[FfFilterGraph.CopyFilterAssetToWorkingDirectory]]
- [[FfFilterGraph.TryGetRelativePathSafe]]
- [[FfFilterGraph.FormatDouble]]
- [[FfFilterGraph.FormatDecibels]]
- [[FfFilterGraph.FormatFraction]]
- [[FfFilterGraph.EnsureDefaultFormatClause]]


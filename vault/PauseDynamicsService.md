---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "Ams.Core.Prosody.IPauseDynamicsService"
member_count: 18
dependency_count: 0
pattern: "service"
tags:
  - class
  - pattern/service
---

# PauseDynamicsService

> Class in `Ams.Core.Prosody`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`

**Implements**:
- [[IPauseDynamicsService]]

## Properties
- `TargetEpsilon`: double
- `IntraSentenceFloorDuration`: double
- `IntraSentenceMaxShrinkSeconds`: double
- `IntraSentenceEdgeGuardSeconds`: double
- `IntraSentenceMinRatio`: double

## Members
- [[PauseDynamicsService.AnalyzeChapter]]
- [[PauseDynamicsService.PlanTransforms]]
- [[PauseDynamicsService.Apply]]
- [[PauseDynamicsService.Execute]]
- [[PauseDynamicsService.BuildInterSentenceSpans]]
- [[PauseDynamicsService.BuildIntraSentenceSpans]]
- [[PauseDynamicsService.GetPunctuationTimes]]
- [[PauseDynamicsService.IsIntraSentencePunctuation]]
- [[PauseDynamicsService.ExtractTimesFromScript]]
- [[PauseDynamicsService.ExtractTimesFromBookWords]]
- [[PauseDynamicsService.MatchSilencesToPunctuation]]
- [[PauseDynamicsService.BuildWordCenters]]
- [[PauseDynamicsService.DistanceToInterval]]
- [[PauseDynamicsService.IsReliableSentence]]
- [[PauseDynamicsService.BuildSentenceParagraphMap]]
- [[PauseDynamicsService.BuildHeadingParagraphSet]]
- [[PauseDynamicsService.FilterParagraphZeroAdjustments]]
- [[PauseDynamicsService.CloneBaseline]]


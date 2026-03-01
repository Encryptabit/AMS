---
namespace: "Ams.Core.Processors.Diffing"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Processors.Diffing.TextDiffScoringOptions>"
member_count: 1
dependency_count: 0
pattern: ~
tags:
  - class
---

# TextDiffScoringOptions

> Record in `Ams.Core.Processors.Diffing`

**Path**: `Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs`

**Implements**:
- IEquatable

## Properties
- `ReferenceTokens`: IReadOnlyList<string>?
- `HypothesisTokens`: IReadOnlyList<string>?
- `ReferencePhonemeVariants`: IReadOnlyList<string[]?>?
- `HypothesisPhonemeVariants`: IReadOnlyList<string[]?>?
- `UseExactPhonemeEquivalence`: bool

## Members


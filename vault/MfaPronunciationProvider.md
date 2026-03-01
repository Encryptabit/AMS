---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "Ams.Core.Runtime.Book.IPronunciationProvider"
member_count: 10
dependency_count: 1
pattern: ~
tags:
  - class
---

# MfaPronunciationProvider

> Class in `Ams.Core.Application.Mfa`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs`

**Implements**:
- [[IPronunciationProvider]]

## Dependencies
- [[Ams.Core.Application.Mfa.MfaService_]] (`mfaService`)

## Properties
- `_mfaService`: MfaService
- `_g2pModel`: string
- `MaxPronunciationsPerLexeme`: int
- `_nextG2pInvocationId`: int
- `VariantSuffixPattern`: Regex

## Members
- [[MfaPronunciationProvider..ctor]]
- [[MfaPronunciationProvider.GetPronunciationsAsync]]
- [[MfaPronunciationProvider.RunG2pWithProgressAsync]]
- [[MfaPronunciationProvider.GetFileState]]
- [[MfaPronunciationProvider.FormatElapsed]]
- [[MfaPronunciationProvider.BuildInvocationTag]]
- [[MfaPronunciationProvider.NormalizeVariantKey]]
- [[MfaPronunciationProvider.ComposeLexemePronunciations]]
- [[MfaPronunciationProvider.MergePronunciationMaps]]
- [[MfaPronunciationProvider.ExpandCombinations]]


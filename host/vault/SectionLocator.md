---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs"
access_modifier: "public"
base_class: ~
interfaces: []
member_count: 15
dependency_count: 0
pattern: ~
tags:
  - class
---

# SectionLocator

> Class in `Ams.Core.Processors.Alignment.Anchors`

**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs`

## Properties
- `HeadingKeywords`: HashSet<string>
- `LeadingChapterKeywords`: HashSet<string>
- `SpelledUnits`: Dictionary<string, int>
- `SpelledOrdinals`: Dictionary<string, int>
- `SpelledTens`: Dictionary<string, int>
- `RomanMap`: Dictionary<char, int>

## Members
- [[SectionLocator.DetectSection]]
- [[SectionLocator.DetectSectionWindow]]
- [[SectionLocator.ResolveSectionByTitle]]
- [[SectionLocator.LongestCommonPrefix]]
- [[SectionLocator.NormalizeTokens]]
- [[SectionLocator.CollapseNumberTokens]]
- [[SectionLocator.TryParseCombinedNumber]]
- [[SectionLocator.TryParseIntToken]]
- [[SectionLocator.TryParseRoman]]
- [[SectionLocator.ExtractLeadingNumber]]
- [[SectionLocator.TryParseEmbeddedChapterNumber]]
- [[SectionLocator.TryParseFullNumber]]
- [[SectionLocator.BuildSectionLookup]]
- [[SectionLocator.BuildNormalizedVariants]]
- [[SectionLocator.TrimLeadingKeywords]]


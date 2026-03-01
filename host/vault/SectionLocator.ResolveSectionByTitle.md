---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs"
access_modifier: "public"
complexity: 13
fan_in: 5
fan_out: 5
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
---
# SectionLocator::ResolveSectionByTitle
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs`

## Summary
**Resolves a chapter/section label (e.g., filename stem) to a `SectionRange` using aggressive token normalization plus numeric and normalized-title lookup heuristics.**

ResolveSectionByTitle performs title-based section resolution by normalizing the input label (`NormalizeTokens`), collapsing numeric expressions (`CollapseNumberTokens`), and building precomputed section lookup maps via `BuildSectionLookup`. It first attempts a fast numeric disambiguation path: extract a leading chapter number (`ExtractLeadingNumber`) and return immediately only when that number maps to exactly one section candidate. If numeric matching is ambiguous or absent, it generates normalized label variants (`BuildNormalizedVariants`) and probes `lookup.ByNormalized`, returning a unique match or, for multi-match buckets, the candidate whose `NormalizedOriginal` exactly equals the variant. It returns `null` when no confident unique/equivalent candidate is found.


#### [[SectionLocator.ResolveSectionByTitle]]
##### What it does:
<member name="M:Ams.Core.Processors.Alignment.Anchors.SectionLocator.ResolveSectionByTitle(Ams.Core.Runtime.Book.BookIndex,System.String)">
    <summary>
    Resolve a section directly from an audio filename stem or arbitrary chapter label.
    Applies aggressive normalization so titles like
    "11- Aboard the Bounty" and "Chapter Eleven – Aboard the Bounty" map to the same section.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static SectionRange ResolveSectionByTitle(BookIndex book, string chapterLabel)
```

**Calls ->**
- [[SectionLocator.BuildNormalizedVariants]]
- [[SectionLocator.BuildSectionLookup]]
- [[SectionLocator.CollapseNumberTokens]]
- [[SectionLocator.ExtractLeadingNumber]]
- [[SectionLocator.NormalizeTokens]]

**Called-by <-**
- [[MergeTimingsCommand.ResolveChapterWordWindow]]
- [[ChapterContext.GetOrResolveSection]]
- [[ChapterDiscoveryService.DiscoverChaptersCore]]
- [[ChapterManager.TryResolveSection]]
- [[SectionLocatorTests.ResolveSectionByTitle_NormalizesNumbers]]


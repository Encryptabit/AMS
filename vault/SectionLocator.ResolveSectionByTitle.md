---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs"
access_modifier: "public"
complexity: 13
fan_in: 5
fan_out: 5
tags:
  - method
---
# SectionLocator::ResolveSectionByTitle
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs`


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


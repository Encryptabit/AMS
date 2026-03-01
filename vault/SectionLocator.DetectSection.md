---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs"
access_modifier: "public"
complexity: 14
fan_in: 3
fan_out: 3
tags:
  - method
---
# SectionLocator::DetectSection
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs`


#### [[SectionLocator.DetectSection]]
##### What it does:
<member name="M:Ams.Core.Processors.Alignment.Anchors.SectionLocator.DetectSection(Ams.Core.Runtime.Book.BookIndex,System.Collections.Generic.IReadOnlyList{System.String},System.Int32)">
    <summary>
    Attempts to detect a section using the first few ASR tokens.
    Returns the best matching SectionRange or null if no confident match.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static SectionRange DetectSection(BookIndex book, IReadOnlyList<string> asrTokens, int prefixTokenCount = 8)
```

**Calls ->**
- [[TextNormalizer.Normalize]]
- [[TextNormalizer.TokenizeWords]]
- [[SectionLocator.LongestCommonPrefix]]

**Called-by <-**
- [[AnchorPipeline.ComputeAnchors]]
- [[SectionLocator.DetectSectionWindow]]
- [[SectionLocatorTests.Detects_Chapter_From_Asr_Prefix]]


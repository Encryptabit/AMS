---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs"
access_modifier: "public"
complexity: 14
fan_in: 3
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/validation
---
# SectionLocator::DetectSection
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs`

## Summary
**Chooses the most likely book section from an ASR opening-token prefix using normalized prefix-match scoring with keyword-aware confidence gating.**

DetectSection attempts section identification by scoring the first `prefixTokenCount` ASR tokens (minimum 1) against each section title using normalized word-token prefixes. It normalizes both ASR prefix and section titles through `TextNormalizer.Normalize(..., expandContractions: true, removeNumbers: false)` and `TextNormalizer.TokenizeWords`, then computes `LongestCommonPrefix` per candidate. Matching gets a small heuristic boost (`+1`) when both streams start with the same heading keyword (e.g., `chapter`, `prologue`), and the method keeps the highest-scoring `SectionRange`. It returns that section only if confidence passes a threshold (`>=2` prefix tokens, or `>=1` when ASR starts with a heading keyword); otherwise it returns `null`.


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


---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorPreprocessor.cs"
access_modifier: "public"
complexity: 3
fan_in: 4
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
---
# AnchorPreprocessor::BuildBookView
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorPreprocessor.cs`

## Summary
**Builds a normalized book-token view for anchoring, with sentence metadata and original↔filtered index translation tables.**

BuildBookView creates a normalized, filtered token projection of `BookIndex.Words` while preserving bidirectional index mappings to original word positions. It preallocates `tokens`, `sentIdx`, and `filteredToOriginal` to `book.Totals.Words`, initializes `originalToFiltered` with `-1`, then iterates all words, normalizing each via `AnchorTokenizer.Normalize`. Non-empty normalized tokens are appended, sentence indices copied from `book.Words[i].SentenceIndex`, `filteredToOriginal` records original indices, and `originalToFiltered[i]` is set to the filtered position. It returns a `BookAnchorView` containing filtered tokens, parallel sentence indices, and both mapping arrays.


#### [[AnchorPreprocessor.BuildBookView]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static BookAnchorView BuildBookView(BookIndex book)
```

**Calls ->**
- [[AnchorTokenizer.Normalize]]

**Called-by <-**
- [[AnchorPipeline.ComputeAnchors]]
- [[AnchorComputeService.ComputeAnchorsAsync]]
- [[TranscriptIndexService.BuildTranscriptIndexAsync]]
- [[TranscriptIndexService.BuildWordOperations]]


---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorPreprocessor.cs"
access_modifier: "public"
complexity: 11
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
---
# AnchorPreprocessor::TryMapSectionWindow
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorPreprocessor.cs`

## Summary
**Translates an inclusive original word-range section into an inclusive filtered-token window, if any filtered tokens fall inside the section.**

TryMapSectionWindow maps an original-word section range to filtered-token indices using `view.FilteredToOriginalWord`. It performs a forward scan to find the first filtered index whose original word falls within `[startWord, endWord]`, then (only if found) a reverse scan to find the last matching filtered index. If both bounds exist and are ordered, it assigns `window = (startFiltered, endFiltered)` and returns `true`; otherwise it sets `window = default` and returns `false`.


#### [[AnchorPreprocessor.TryMapSectionWindow]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static bool TryMapSectionWindow(BookAnchorView view, (int startWord, int endWord) section, out (int startFiltered, int endFiltered) window)
```

**Called-by <-**
- [[AnchorPipeline.ComputeAnchors]]


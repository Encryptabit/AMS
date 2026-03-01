---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterContext.cs"
access_modifier: "internal"
complexity: 1
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
---
# ChapterContext::SetDetectedSection
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterContext.cs`

## Summary
**Caches an auto-detected section only if a section has not already been resolved.**

`SetDetectedSection` performs a one-time cache assignment for `_resolvedSection` using null-coalescing assignment (`_resolvedSection ??= section`). It preserves any previously resolved/overridden section and ignores subsequent calls once set. The method has no validation or logging and only mutates internal section-resolution state.


#### [[ChapterContext.SetDetectedSection]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal void SetDetectedSection(SectionRange section)
```

**Called-by <-**
- [[AnchorComputeService.ComputeAnchorsAsync]]
- [[TranscriptIndexService.BuildTranscriptIndexAsync]]


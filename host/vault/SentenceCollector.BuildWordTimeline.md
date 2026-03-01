---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
---
# SentenceCollector::BuildWordTimeline
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`

## Summary
**Builds the initial per-word sentence timeline by assigning book words evenly spaced timestamps across the sentence’s original timing window.**

BuildWordTimeline clamps `sentence.BookRange.Start/End` to valid indices in `bookIndex.Words`, computes `wordCount`, and derives a uniform per-word `step` from `OriginalTiming` duration. It iterates each word in range, reads `WordIndex` and `Text` from `bookIndex.Words[start + i]`, and appends a `SentenceWordElement` to `_timeline`. Each element starts with identical original/current timing fields, and the last word’s end is explicitly set to `sentenceEnd` to prevent interval drift.


#### [[SentenceCollector.BuildWordTimeline]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void BuildWordTimeline(SentenceAlign sentence, BookIndex bookIndex)
```

**Called-by <-**
- [[SentenceCollector..ctor]]


---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# PauseDynamicsService::ExtractTimesFromBookWords
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`

## Summary
**Estimates intra-sentence punctuation times from book-word tokens by projecting punctuation-bearing words onto sentence-relative timing.**

`ExtractTimesFromBookWords` derives punctuation anchor times from `bookIndex.Words` within `sentence.BookRange`. It clamps range indices to valid word bounds, exits early when sentence duration is non-positive, then linearly partitions sentence time across words to compute per-word start/end and center timestamps. For each token, every character matching `IsIntraSentencePunctuation` contributes the word-center time to the result list. The method returns all collected anchors without deduplication.


#### [[PauseDynamicsService.ExtractTimesFromBookWords]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<double> ExtractTimesFromBookWords(SentenceAlign sentence, BookIndex bookIndex)
```

**Calls ->**
- [[PauseDynamicsService.IsIntraSentencePunctuation]]

**Called-by <-**
- [[PauseDynamicsService.GetPunctuationTimes]]


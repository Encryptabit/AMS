---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# TranscriptIndexService::BuildBookPhonemeView
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs`

## Summary
**Projects book word phonemes onto the filtered token index space used by alignment logic.**

This private static mapper builds a phoneme matrix aligned to filtered book-token positions. It allocates `string[filteredCount][]`, translates each filtered index through `filteredToOriginal`, and copies `book.Words[originalIndex].Phonemes` when the mapped index is in range. Missing phoneme arrays and out-of-range mappings are normalized to `Array.Empty<string>()`, so callers always receive non-null inner arrays.


#### [[TranscriptIndexService.BuildBookPhonemeView]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string[][] BuildBookPhonemeView(BookIndex book, IReadOnlyList<int> filteredToOriginal, int filteredCount)
```

**Called-by <-**
- [[TranscriptIndexService.BuildTranscriptIndexAsync]]


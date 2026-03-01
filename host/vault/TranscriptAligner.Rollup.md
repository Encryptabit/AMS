---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
---
# TranscriptAligner::Rollup
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`

## Summary
**Provides a simplified API for sentence/paragraph rollup when optional `BookIndex` and `AsrResponse` context are not available.**

This overload is a convenience wrapper that materializes no additional logic and immediately delegates to the fuller `Rollup` overload with `book` and `asr` passed as `null`. It preserves the input `ops`, `bookSentences`, and `bookParagraphs` and returns the delegated tuple `(sents, paras)` unchanged.


#### [[TranscriptAligner.Rollup]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static (List<SentenceAlign> sents, List<ParagraphAlign> paras) Rollup(IReadOnlyList<WordAlign> ops, IReadOnlyList<(int Id, int Start, int End)> bookSentences, IReadOnlyList<(int Id, int Start, int End)> bookParagraphs)
```

**Calls ->**
- [[TranscriptAligner.Rollup_2]]


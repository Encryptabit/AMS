---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
---
# TranscriptAligner::Rollup
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`


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


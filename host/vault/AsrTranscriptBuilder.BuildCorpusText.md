---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Asr/AsrTranscriptBuilder.cs"
access_modifier: "public"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AsrTranscriptBuilder::BuildCorpusText
**Path**: `Projects/AMS/host/Ams.Core/Asr/AsrTranscriptBuilder.cs`

## Summary
**Convert an ASR response into newline-delimited corpus text derived from parsed sentence units.**

`BuildCorpusText` validates input (`ArgumentNullException.ThrowIfNull`), delegates sentence extraction to `BuildSentences(response)`, and returns `string.Empty` when no sentences are produced. When sentences exist, it emits a corpus block by joining them with `Environment.NewLine`, yielding one sentence per line. The method contains no parsing logic itself and serves as a formatting wrapper over the sentence builder.


#### [[AsrTranscriptBuilder.BuildCorpusText]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static string BuildCorpusText(AsrResponse response)
```

**Calls ->**
- [[AsrTranscriptBuilder.BuildSentences]]

**Called-by <-**
- [[GenerateTranscriptCommand.PersistResponse]]
- [[ChapterManager.CreateContext]]


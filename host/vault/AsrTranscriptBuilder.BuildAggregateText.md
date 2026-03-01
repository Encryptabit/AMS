---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Asr/AsrTranscriptBuilder.cs"
access_modifier: "private"
complexity: 10
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AsrTranscriptBuilder::BuildAggregateText
**Path**: `Projects/AMS/host/Ams.Core/Asr/AsrTranscriptBuilder.cs`

## Summary
**Build a single normalized aggregate transcript string from ASR segments or, if absent, from indexed words.**

`BuildAggregateText` composes one whitespace-delimited transcript string from an `AsrResponse`, preferring `Segments` text when present and falling back to per-word reconstruction via `GetWord`. It returns `string.Empty` immediately when both `Segments` and `WordCount` are empty, then appends non-blank units into a `StringBuilder` with single-space separators and local trimming for segment text. In segment mode it iterates `response.Segments`, skipping null/whitespace `segment.Text`; in word mode it loops `0..WordCount-1`, skips blank words, and appends each token. Output order preserves the original segment/word sequence.


#### [[AsrTranscriptBuilder.BuildAggregateText]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string BuildAggregateText(AsrResponse response)
```

**Calls ->**
- [[AsrResponse.GetWord]]

**Called-by <-**
- [[AsrTranscriptBuilder.BuildSentences]]


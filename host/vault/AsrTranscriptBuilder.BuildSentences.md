---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Asr/AsrTranscriptBuilder.cs"
access_modifier: "public"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AsrTranscriptBuilder::BuildSentences
**Path**: `Projects/AMS/host/Ams.Core/Asr/AsrTranscriptBuilder.cs`

## Summary
**Parse ASR aggregate text into an ordered list of cleaned sentence strings with robust empty/fallback handling.**

`BuildSentences` derives sentence strings from an `AsrResponse` by first validating input and assembling aggregate text via `BuildAggregateText`. It short-circuits to `Array.Empty<string>()` when aggregate text is null/whitespace, then normalizes internal whitespace to single spaces (`Regex.Replace(..., "\\s+", " ")`) and trims. It splits normalized text using `SentenceBoundaryRegex` (punctuation-plus-capital lookaround), trims each piece, and collects non-empty sentences in order. If splitting yields nothing, it falls back to returning the full normalized text as a single sentence.


#### [[AsrTranscriptBuilder.BuildSentences]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IReadOnlyList<string> BuildSentences(AsrResponse response)
```

**Calls ->**
- [[AsrTranscriptBuilder.BuildAggregateText]]

**Called-by <-**
- [[AsrTranscriptBuilder.BuildCorpusText]]


---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AsrProcessor::NormalizeTokenText
**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

## Summary
**Normalizes tokenizer-specific boundary glyphs into plain-spaced token text for downstream word assembly.**

`NormalizeTokenText` canonicalizes raw token text by replacing Whisper boundary markers (`▁`, `Ġ`, `Ċ`) with spaces and then trimming leading/trailing whitespace. It preserves other characters and performs no casing or punctuation normalization.


#### [[AsrProcessor.NormalizeTokenText]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string NormalizeTokenText(string text)
```

**Called-by <-**
- [[AsrProcessor.AggregateTokens]]


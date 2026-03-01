---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "private"
complexity: 4
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# TranscriptAligner::AppendNormalized
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`

## Summary
**Mutates a buffer with a lowercase alphanumeric-only normalization of an input token.**

`AppendNormalized` is a low-level normalizer that appends only alphanumeric characters from `text` into the provided `StringBuilder`. It no-ops for null or empty input, then iterates each character and filters with `char.IsLetterOrDigit`. Accepted characters are appended as `char.ToLowerInvariant(c)`, removing punctuation, whitespace, and case distinctions.


#### [[TranscriptAligner.AppendNormalized]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void AppendNormalized(StringBuilder builder, string text)
```

**Called-by <-**
- [[TranscriptAligner.BuildNormalizedWordString]]
- [[TranscriptAligner.BuildNormalizedWordString_2]]


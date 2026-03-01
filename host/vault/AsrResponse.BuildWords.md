---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Asr/AsrModels.cs"
access_modifier: "private"
complexity: 8
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AsrResponse::BuildWords
**Path**: `Projects/AMS/host/Ams.Core/Asr/AsrModels.cs`

## Summary
**Generate the normalized word sequence for an ASR response from token timings when available, or by splitting segment text otherwise.**

`BuildWords` constructs the response’s canonical word list with a token-first strategy. If `Tokens` is non-empty, it projects `t.Word` directly to an array; otherwise it falls back to segment text parsing, returning empty when `Segments` is empty. In segment mode, it skips blank `segment.Text`, splits on whitespace (`' '`, `'\t'`, `'\r'`, `'\n'`) with `RemoveEmptyEntries`, appends words to a `List<string>`, and returns either `Array.Empty<string>()` or `list.ToArray()` depending on collected count. The method is allocation-conscious and deterministic in output order.


#### [[AsrResponse.BuildWords]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private IReadOnlyList<string> BuildWords()
```


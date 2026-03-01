---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "public"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# TranscriptAligner::Equivalent
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`

## Summary
**Checks whether two tokens should be treated as equivalent via exact match or configured bidirectional alias mapping.**

Equivalent evaluates symmetric token equivalence using a direct equality fast path plus one-hop dictionary mappings. It returns true if `a == b`, if `equiv[a] == b`, or if `equiv[b] == a`, using `TryGetValue` for both lookup directions. No normalization or transitive chaining is applied; only exact string equality and explicit pair mappings are considered.


#### [[TranscriptAligner.Equivalent]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static bool Equivalent(string a, string b, IReadOnlyDictionary<string, string> equiv)
```

**Called-by <-**
- [[TranscriptAligner.SubCost]]


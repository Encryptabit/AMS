---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorTokenizer.cs"
access_modifier: "public"
complexity: 5
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AnchorTokenizer::Normalize
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorTokenizer.cs`

## Summary
**Produces a stable anchor token by lowercasing and retaining only Unicode letters and digits from the input string.**

Normalize canonicalizes a single token by stripping all non-alphanumeric characters and lowercasing retained characters with invariant casing. It fast-paths null/empty input to `string.Empty`, iterates over `s.AsSpan()`, writes accepted chars into a stack-allocated buffer (`stackalloc char[s.Length]`), and tracks output length with `k`. The method returns `string.Empty` when no letters/digits are present (e.g., punctuation-only input), otherwise constructs a string from `buf[..k]`.


#### [[AnchorTokenizer.Normalize]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static string Normalize(string s)
```

**Called-by <-**
- [[AnchorPreprocessor.BuildAsrView]]
- [[AnchorPreprocessor.BuildBookView]]


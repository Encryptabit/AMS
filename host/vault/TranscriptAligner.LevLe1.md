---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "public"
complexity: 9
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# TranscriptAligner::LevLe1
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`

## Summary
**Determines whether two tokens are identical or differ by at most one single-character edit.**

LevLe1 is a linear-time check for whether two strings are within Levenshtein edit distance 1. It first handles exact equality and quickly rejects when length difference exceeds one, then walks both strings with two pointers while counting mismatches/edits. On mismatch it increments edit count and advances pointers according to relative lengths (substitution for equal lengths, insertion/deletion otherwise), aborting once edits exceed one. After the main loop, it accounts for any trailing unmatched characters and returns true only if total edits are ≤ 1.


#### [[TranscriptAligner.LevLe1]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static bool LevLe1(string a, string b)
```

**Called-by <-**
- [[TranscriptAligner.SubCost]]


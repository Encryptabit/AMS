---
namespace: "Ams.Core.Processors.Diffing"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# TextDiffAnalyzer::MapOperation
**Path**: `Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs`

## Summary
**Converts external diff operation enums into the service’s canonical operation string codes.**

`MapOperation` translates `DiffMatchPatch.Operation` enum values into the internal hydrated diff op labels. It maps `EQUAL` to `"equal"`, `INSERT` to `"insert"`, and `DELETE` to `"delete"`, with a fallback/default branch for unexpected values. This provides a stable string representation used in hydrated diff payloads.


#### [[TextDiffAnalyzer.MapOperation]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string MapOperation(Operation op)
```

**Called-by <-**
- [[TextDiffAnalyzer.Analyze_2]]


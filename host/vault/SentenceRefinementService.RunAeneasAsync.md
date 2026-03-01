---
namespace: "Ams.Core.Pipeline"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Pipeline/SentenceRefinementService.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/async
  - llm/data-access
  - llm/utility
  - llm/error-handling
---
# SentenceRefinementService::RunAeneasAsync
**Path**: `Projects/AMS/host/Ams.Core/Pipeline/SentenceRefinementService.cs`

## Summary
**Executes the Aeneas CLI for the provided audio and sentence text lines, returning aligned begin/end timestamps per fragment.**

RunAeneasAsync stages input lines into a temp `sentences.txt`, launches `python -m aeneas.tools.execute_task` (using `AENEAS_PYTHON` or `python`) to align `audioPath`, and awaits process completion with the service timeout via `CancellationTokenSource`. It captures stderr, throws `InvalidOperationException` on process start failure or non-zero exit, then reads `alignment.json` and maps `fragments[*].begin/end` into a `List<(double begin, double end)>`. The local `ParseDouble(JsonElement)` handles both string and numeric JSON values, and a `finally` block best-effort deletes the temp directory while swallowing cleanup exceptions.


#### [[SentenceRefinementService.RunAeneasAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<List<(double begin, double end)>> RunAeneasAsync(string audioPath, List<string> lines, string language)
```

**Calls ->**
- [[ParseDouble]]

**Called-by <-**
- [[SentenceRefinementService.RefineAsync]]


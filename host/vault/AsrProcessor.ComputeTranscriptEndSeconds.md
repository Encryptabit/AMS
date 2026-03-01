---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AsrProcessor::ComputeTranscriptEndSeconds
**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

## Summary
**Finds the transcript’s effective end timestamp by taking the furthest end across segments and tokens.**

`ComputeTranscriptEndSeconds` computes the maximum observed end time across both segment- and token-level ASR outputs. It initializes `maxEnd` to `0`, scans `response.Segments` for the largest `EndSec`, then scans `response.Tokens` using `token.StartTime + Math.Max(0, token.Duration)` to handle non-negative token extents. The method returns the greater of all discovered endpoints, yielding a unified transcript coverage tail timestamp.


#### [[AsrProcessor.ComputeTranscriptEndSeconds]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double ComputeTranscriptEndSeconds(AsrResponse response)
```

**Called-by <-**
- [[AsrProcessor.ShouldRetryWithoutDtw]]


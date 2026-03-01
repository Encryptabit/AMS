---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 1
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/data-access
---
# InteractiveState::GetParagraphSentenceCount
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Returns the number of sentences associated with a specified paragraph ID by reusing the paragraph sentence-ID retrieval logic.**

GetParagraphSentenceCount(int paragraphId) in ValidateTimingSession.InteractiveState is a thin O(1) delegating method that calls GetParagraphSentenceIds(paragraphId) and derives the sentence total from that result. It acts as the shared count accessor used by BuildParagraphLabel and BuildTreeSummary, so both consumers rely on the same paragraph-to-sentence lookup path.


#### [[InteractiveState.GetParagraphSentenceCount]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public int GetParagraphSentenceCount(int paragraphId)
```

**Calls ->**
- [[InteractiveState.GetParagraphSentenceIds]]

**Called-by <-**
- [[InteractiveState.BuildParagraphLabel]]
- [[TimingRenderer.BuildTreeSummary]]


---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapModels.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
---
# SentencePauseMap::UpdateTiming
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapModels.cs`

## Summary
**Updates the sentence pause map’s current timing state.**

`UpdateTiming` is a direct state mutator that replaces `CurrentTiming` with the supplied `SentenceTiming` value. It performs no validation or normalization and does not recompute timeline elements or statistics. The method is O(1) and only affects the current timing snapshot.


#### [[SentencePauseMap.UpdateTiming]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void UpdateTiming(SentenceTiming timing)
```


---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseModels.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/factory
  - llm/utility
---
# PausePolicy::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseModels.cs`

## Summary
**Creates a `PausePolicy` preconfigured with standard default pause windows and chapter/tail timing values.**

The parameterless constructor delegates to the main `PausePolicy(...)` constructor, supplying built-in defaults for all policy components. It instantiates default pause windows (`Comma: 0.20–0.50`, `Sentence: 0.60–1.00`, `Paragraph: 1.10–1.40`) and sets fixed chapter/tail durations (`headOfChapter: 0.75`, `postChapterRead: 1.50`, `tail: 3.00`). Other optional tuning parameters use their defaults from the target constructor signature.


#### [[PausePolicy..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public PausePolicy()
```


---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseModels.cs"
access_modifier: "public"
complexity: 1
fan_in: 6
fan_out: 0
tags:
  - method
  - llm/factory
  - llm/utility
---
# PausePolicyPresets::House
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseModels.cs`

## Summary
**Provides the standard “house” pause policy preset by constructing the default `PausePolicy` instance.**

`House()` is a one-line preset factory that returns `new PausePolicy()`, delegating all configuration to the default `PausePolicy` constructor. It performs no parameter handling, validation, or post-processing itself. The resulting policy uses whatever baseline window and tuning defaults are encoded in `PausePolicy`’s parameterless initialization path.


#### [[PausePolicyPresets.House]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static PausePolicy House()
```

**Called-by <-**
- [[ValidateCommand.CreateTimingInitCommand]]
- [[PausePolicyResolver.Resolve]]
- [[PauseMapBuilder.Build]]
- [[FileArtifactResolver.LoadPausePolicy]]
- [[ChapterDocuments..ctor]]
- [[PauseDynamicsServiceTests.PlanTransforms_CompressesSentencePauseOutsideWindow]]


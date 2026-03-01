---
namespace: "Ams.Core.Processors.Alignment.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Mfa/TextGridParser.cs"
access_modifier: "public"
complexity: 1
fan_in: 5
fan_out: 1
tags:
  - method
---
# TextGridParser::ParseWordIntervals
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Mfa/TextGridParser.cs`


#### [[TextGridParser.ParseWordIntervals]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IReadOnlyList<TextGridInterval> ParseWordIntervals(string textGridPath)
```

**Calls ->**
- [[TextGridParser.ParseIntervals]]

**Called-by <-**
- [[PipelineCommand.LoadMfaSilences]]
- [[ValidateTimingSession.TryLoadMfaSilences]]
- [[MergeTimingsCommand.ExecuteAsync]]
- [[FileArtifactResolver.LoadTextGrid]]
- [[PickupMfaRefinementService.RefineAsrTimingsAsync]]


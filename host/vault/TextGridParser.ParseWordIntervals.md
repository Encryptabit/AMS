---
namespace: "Ams.Core.Processors.Alignment.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/TextGridParser.cs"
access_modifier: "public"
complexity: 1
fan_in: 5
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# TextGridParser::ParseWordIntervals
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/TextGridParser.cs`

## Summary
**Parses and returns intervals from the TextGrid tier named `words` (case-insensitive).**

ParseWordIntervals is a thin selector over the shared TextGrid parser pipeline. It delegates to `ParseIntervals(textGridPath, tierPredicate)` with a predicate that captures only tiers whose `name` equals `"words"` using `StringComparison.OrdinalIgnoreCase`. All file validation and line-level interval extraction logic is handled by `ParseIntervals`; this method just fixes the tier filter for word intervals.


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


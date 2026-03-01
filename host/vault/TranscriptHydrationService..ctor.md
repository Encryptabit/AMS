---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/di
  - llm/utility
  - llm/validation
---
# TranscriptHydrationService::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs`

## Summary
**Creates a transcript hydration service with either an injected pronunciation provider or a null-object default.**

The constructor initializes `TranscriptHydrationService` with an optional pronunciation dependency and falls back to `NullPronunciationProvider.Instance` when none is supplied. It stores the resolved provider in `_pronunciationProvider` for later fallback phoneme generation during hydration. No other state or side effects occur during construction.


#### [[TranscriptHydrationService..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public TranscriptHydrationService(IPronunciationProvider pronunciationProvider = null)
```


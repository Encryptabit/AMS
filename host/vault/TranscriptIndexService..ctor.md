---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/di
  - llm/utility
---
# TranscriptIndexService::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs`

## Summary
**Creates a `TranscriptIndexService` with either an injected pronunciation provider or a built-in no-op provider.**

The constructor initializes the service’s pronunciation dependency with a null-safe fallback. It assigns the private `_pronunciationProvider` field to the supplied `IPronunciationProvider` when present, otherwise defaults to `NullPronunciationProvider.Instance`. This makes downstream transcript-index building logic operate without null checks for pronunciation lookups.


#### [[TranscriptIndexService..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public TranscriptIndexService(IPronunciationProvider pronunciationProvider = null)
```


---
namespace: "Ams.Core.Services.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Documents/DocumentService.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/di
---
# DocumentService::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Services/Documents/DocumentService.cs`

## Summary
**Initializes `DocumentService` with optional pronunciation and caching dependencies used by later document-index operations.**

This constructor is a direct dependency assignment point for `DocumentService`. It accepts optional `IPronunciationProvider` and `IBookCache` collaborators and stores them in `_pronunciationProvider` and `_cache` without creating defaults or performing validation. Runtime behavior (phoneme enrichment and cache usage) is therefore controlled by whether these injected dependencies are null.


#### [[DocumentService..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public DocumentService(IPronunciationProvider pronunciationProvider = null, IBookCache cache = null)
```


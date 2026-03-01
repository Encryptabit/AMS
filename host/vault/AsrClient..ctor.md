---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Asr/AsrClient.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/factory
  - llm/utility
---
# AsrClient::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Asr/AsrClient.cs`

## Summary
**Initialize an ASR HTTP client with a normalized base URL and environment-configurable request timeout.**

The `AsrClient` constructor normalizes the service endpoint by trimming any trailing slash from `baseUrl` and storing it in `_baseUrl`. It then instantiates a new internal `HttpClient` and sets its `Timeout` from `ResolveTimeout()`, which reads `AMS_ASR_HTTP_TIMEOUT_SECONDS` and falls back to 15 minutes when unset/invalid. No external `HttpClient` is injected, so each client instance owns its own transport lifetime.


#### [[AsrClient..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public AsrClient(string baseUrl = "http://localhost:8000")
```

**Calls ->**
- [[AsrClient.ResolveTimeout]]


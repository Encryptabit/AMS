---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Asr/AsrClient.cs"
access_modifier: "public"
complexity: 2
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/async
  - llm/data-access
  - llm/error-handling
---
# AsrClient::IsHealthyAsync
**Path**: `Projects/AMS/host/Ams.Core/Asr/AsrClient.cs`

## Summary
**Probe the ASR service health endpoint and report availability as a boolean without surfacing exceptions.**

`IsHealthyAsync` checks service liveness by issuing an async `GET` to `"{_baseUrl}/health"` on the internal `HttpClient` after disposal guarding (`ObjectDisposedException.ThrowIf`). It returns `response.IsSuccessStatusCode` when the request completes, and catches all exceptions to return `false` instead of propagating transport/cancellation/HTTP failures. The method is intentionally best-effort and non-throwing for health probes.


#### [[AsrClient.IsHealthyAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default(CancellationToken))
```

**Called-by <-**
- [[GenerateTranscriptCommand.RunNemoAsync]]
- [[AsrProcessSupervisor.IsHealthyAsync]]


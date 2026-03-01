---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Asr/AsrClient.cs"
access_modifier: "public"
complexity: 6
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/async
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# AsrClient::TranscribeAsync
**Path**: `Projects/AMS/host/Ams.Core/Asr/AsrClient.cs`

## Summary
**Submit an audio file to the ASR service and return the parsed transcription response with explicit failure handling for missing files, HTTP errors, deserialization failures, and timeouts.**

`TranscribeAsync` performs an async HTTP transcription call after guarding object lifetime (`ObjectDisposedException.ThrowIf`) and validating that `audioPath` exists (`FileNotFoundException` if not). It builds an `AsrRequest` using `Path.GetFullPath(audioPath)`, serializes it to JSON, and `POST`s to `"{_baseUrl}/asr"` with cancellation support. On non-success status it reads the error payload and throws `HttpRequestException`; on success it deserializes `AsrResponse` and throws `InvalidOperationException` if deserialization returns null. It maps request timeouts/canceled HTTP operations (when not caller-canceled) to a `TimeoutException` via a filtered `TaskCanceledException` catch block.


#### [[AsrClient.TranscribeAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<AsrResponse> TranscribeAsync(string audioPath, string model = null, string language = "en", CancellationToken cancellationToken = default(CancellationToken))
```

**Called-by <-**
- [[GenerateTranscriptCommand.RunNemoAsync]]


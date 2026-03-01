---
namespace: "Ams.Tests"
project: "Ams.Tests"
source_file: "Projects/AMS/host/Ams.Tests/BookParsingTests.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 5
tags:
  - method
  - llm/async
  - llm/validation
  - llm/data-access
  - llm/factory
---
# BookIndexAcceptanceTests::Canonical_RoundTrip_DeterministicBytes_WithCache
**Path**: `Projects/AMS/host/Ams.Tests/BookParsingTests.cs`

## Summary
**Verify that parsing and caching a book index yields stable, deterministic serialized bytes across cache-assisted round-trips.**

`Canonical_RoundTrip_DeterministicBytes_WithCache` is an async acceptance test that exercises a canonical round-trip path with an explicit cache layer. It builds the index via `BuildBookIndexAsync`, creates cache state with `CreateBookCache`, reads/writes cached entries using `GetAsync`/`SetAsync`, and parses source content through `ParseBookAsync` to confirm cached round-trips produce deterministic bytes.


#### [[BookIndexAcceptanceTests.Canonical_RoundTrip_DeterministicBytes_WithCache]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task Canonical_RoundTrip_DeterministicBytes_WithCache()
```

**Calls ->**
- [[DocumentProcessor.BuildBookIndexAsync]]
- [[DocumentProcessor.CreateBookCache]]
- [[DocumentProcessor.ParseBookAsync]]
- [[IBookCache.GetAsync]]
- [[IBookCache.SetAsync]]


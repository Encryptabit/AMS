---
namespace: "<global namespace>"
project: "Ams.Tests"
source_file: "Projects/AMS/host/Ams.Tests/AnchorDiscoveryTests.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/factory
---
# SectionLocatorTests::MakeBookIndex
**Path**: `Projects/AMS/host/Ams.Tests/AnchorDiscoveryTests.cs`

## Summary
**Construct a reusable `BookIndex` fixture for section-location tests.**

`SectionLocatorTests.MakeBookIndex()` is a private static, low-complexity (`2`) helper that builds and returns a `BookIndex` instance used as shared test fixture data. The method is structured as setup code (construction/initialization) rather than logic-heavy behavior, with minimal branching. It is reused by `Detects_Chapter_From_Asr_Prefix` and `ResolveSectionByTitle_NormalizesNumbers` so both tests exercise section resolution against the same canonical index.


#### [[SectionLocatorTests.MakeBookIndex]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static BookIndex MakeBookIndex()
```

**Called-by <-**
- [[SectionLocatorTests.Detects_Chapter_From_Asr_Prefix]]
- [[SectionLocatorTests.ResolveSectionByTitle_NormalizesNumbers]]


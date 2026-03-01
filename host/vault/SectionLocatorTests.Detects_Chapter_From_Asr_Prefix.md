---
namespace: "<global namespace>"
project: "Ams.Tests"
source_file: "Projects/AMS/host/Ams.Tests/AnchorDiscoveryTests.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 2
tags:
  - method
  - llm/validation
---
# SectionLocatorTests::Detects_Chapter_From_Asr_Prefix
**Path**: `Projects/AMS/host/Ams.Tests/AnchorDiscoveryTests.cs`

## Summary
**It verifies that chapter detection works when the input begins with an ASR-style prefix.**

In `SectionLocatorTests`, `Detects_Chapter_From_Asr_Prefix()` is a straight-line Arrange-Act-Assert test (complexity 1): it creates index fixtures via `MakeBookIndex()`, invokes `DetectSection(...)`, and asserts the ASR-prefixed input resolves to a chapter section. The method intentionally avoids branching and isolates a single detection rule.


#### [[SectionLocatorTests.Detects_Chapter_From_Asr_Prefix]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Detects_Chapter_From_Asr_Prefix()
```

**Calls ->**
- [[SectionLocator.DetectSection]]
- [[SectionLocatorTests.MakeBookIndex]]


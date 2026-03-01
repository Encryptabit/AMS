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
# SectionLocatorTests::ResolveSectionByTitle_NormalizesNumbers
**Path**: `Projects/AMS/host/Ams.Tests/AnchorDiscoveryTests.cs`

## Summary
**It verifies that title-based section lookup normalizes numbers and maps the input label to the correct section id.**

This `SectionLocatorTests` method is a parameterized AAA-style test with trivial control flow (complexity 1): it creates test data via `MakeBookIndex`, invokes `ResolveSectionByTitle(label)`, and asserts the resolved identifier equals `expectedId`. The `_NormalizesNumbers` case specifically validates that numeric forms in section titles are normalized before matching, so semantically equivalent labels resolve to the same section.


#### [[SectionLocatorTests.ResolveSectionByTitle_NormalizesNumbers]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void ResolveSectionByTitle_NormalizesNumbers(string label, int expectedId)
```

**Calls ->**
- [[SectionLocator.ResolveSectionByTitle]]
- [[SectionLocatorTests.MakeBookIndex]]


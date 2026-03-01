---
namespace: "<global namespace>"
project: "Ams.Tests"
source_file: "Projects/AMS/host/Ams.Tests/AnchorDiscoveryTests.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/validation
---
# SectionLocatorTests::AnchorSelection_Respects_Book_Window
**Path**: `Projects/AMS/host/Ams.Tests/AnchorDiscoveryTests.cs`

## Summary
**Ensures the anchor-selection algorithm returns only anchors that fall within the configured book window.**

In `SectionLocatorTests`, `AnchorSelection_Respects_Book_Window()` is a low-complexity unit test (complexity 2) that invokes `SelectAnchors` and verifies the selection is constrained by the active book-window bounds. The implementation is likely an arrange/act/assert check where mixed candidate anchors are provided and assertions confirm out-of-window anchors are excluded.


#### [[SectionLocatorTests.AnchorSelection_Respects_Book_Window]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void AnchorSelection_Respects_Book_Window()
```

**Calls ->**
- [[AnchorDiscovery.SelectAnchors_2]]


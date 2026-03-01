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
# AnchorDiscoveryTests::WindowsAreClampedWithSentinels
**Path**: `Projects/AMS/host/Ams.Tests/AnchorDiscoveryTests.cs`

## Summary
**Ensures `BuildWindows` produces boundary-safe windows by using sentinels when indices would otherwise exceed valid bounds.**

`WindowsAreClampedWithSentinels()` in `AnchorDiscoveryTests` appears to be a boundary-condition unit test that calls `BuildWindows` and asserts edge windows are clamped with sentinel markers. Given complexity 2, the implementation is likely a simple arrange/act/assert path with minimal branching around boundary assertions. The test validates clamping behavior at limits rather than the full window-construction algorithm.


#### [[AnchorDiscoveryTests.WindowsAreClampedWithSentinels]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void WindowsAreClampedWithSentinels()
```

**Calls ->**
- [[AnchorDiscovery.BuildWindows]]


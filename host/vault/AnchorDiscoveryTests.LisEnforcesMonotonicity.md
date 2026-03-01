---
namespace: "<global namespace>"
project: "Ams.Tests"
source_file: "Projects/AMS/host/Ams.Tests/AnchorDiscoveryTests.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/validation
  - llm/utility
---
# AnchorDiscoveryTests::LisEnforcesMonotonicity
**Path**: `Projects/AMS/host/Ams.Tests/AnchorDiscoveryTests.cs`

## Summary
**Checks that the LIS-based anchor discovery path returns anchors in monotonic order.**

`LisEnforcesMonotonicity` is a unit-test method on `AnchorDiscoveryTests` with cyclomatic complexity 1, so its implementation is a straight-line assertion flow. It delegates core work to `LisByAp` and validates the resulting LIS/anchor sequence satisfies the monotonicity invariant, with no branching, async flow, or side-effect orchestration.


#### [[AnchorDiscoveryTests.LisEnforcesMonotonicity]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void LisEnforcesMonotonicity()
```

**Calls ->**
- [[AnchorDiscovery.LisByAp]]


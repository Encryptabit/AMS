---
namespace: "<global namespace>"
project: "Ams.Tests"
source_file: "Projects/AMS/host/Ams.Tests/AnchorDiscoveryTests.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/validation
  - llm/utility
---
# SectionLocatorTests::Preprocessor_And_Pipeline_Map_And_Restrict
**Path**: `Projects/AMS/host/Ams.Tests/AnchorDiscoveryTests.cs`

## Summary
**Validates that anchor computation correctly maps preprocessed/pipeline-transformed sections and enforces restriction filtering.**

This test covers the section-locator scenario where preprocessor output and pipeline mapping are both active before restriction logic is applied. It invokes ComputeAnchors and verifies the produced anchors honor mapping transformations while filtering out restricted targets. With complexity 3, the method is a compact Arrange/Act/Assert case focused on the map-and-restrict branch behavior.


#### [[SectionLocatorTests.Preprocessor_And_Pipeline_Map_And_Restrict]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Preprocessor_And_Pipeline_Map_And_Restrict()
```

**Calls ->**
- [[AnchorPipeline.ComputeAnchors]]


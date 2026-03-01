---
namespace: "Ams.Cli.Services"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Services/DspConfigService.cs"
access_modifier: "private"
complexity: 4
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# DspConfigService::NormalizeConfig
**Path**: `Projects/AMS/host/Ams.Cli/Services/DspConfigService.cs`

## Summary
**Normalize a `DspConfig` object into a consistent, persistence-safe shape before and after config I/O flows.**

`NormalizeConfig` is a private static in-place canonicalizer for `DspConfig` that is invoked by both `LoadAsync` and `SaveAsync` to enforce consistent state at both read and write boundaries. Its `void` mutation pattern and reported complexity of 4 imply a compact set of conditional normalization branches applied directly to the provided config instance.


#### [[DspConfigService.NormalizeConfig]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void NormalizeConfig(DspConfig config)
```

**Called-by <-**
- [[DspConfigService.LoadAsync]]
- [[DspConfigService.SaveAsync]]


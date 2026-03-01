---
namespace: "Ams.Core.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Validation/ScriptValidator.cs"
access_modifier: "public"
complexity: 5
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/entry-point
  - llm/data-access
  - llm/async
  - llm/validation
  - llm/error-handling
---
# ScriptValidator::ValidateAsync
**Path**: `Projects/AMS/host/Ams.Core/Validation/ScriptValidator.cs`

## Summary
**Asynchronously loads and validates required input files, constructs typed ASR input, and hands off to the core validator to generate a `ValidationReport`.**

`ValidateAsync` is an async orchestration wrapper around `ScriptValidator.Validate(...)`: it first enforces existence checks for `audioPath`, `scriptPath`, and `asrJsonPath` using `File.Exists`, throwing `FileNotFoundException` for missing inputs. It then reads the script and ASR JSON with `File.ReadAllTextAsync`, deserializes the ASR payload via `JsonSerializer.Deserialize<AsrResponse>`, and throws `InvalidOperationException` if deserialization yields `null`. After input hydration and guards, it delegates to `Validate(audioPath, scriptPath, asrJsonPath, scriptText, asrResponse)` and returns the resulting `ValidationReport`.


#### [[ScriptValidator.ValidateAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<ValidationReport> ValidateAsync(string audioPath, string scriptPath, string asrJsonPath)
```

**Calls ->**
- [[ScriptValidator.Validate]]


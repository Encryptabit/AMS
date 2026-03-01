---
namespace: "Ams.Core.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Artifacts/TimingOverrides.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/factory
  - llm/error-handling
---
# TimingOverridesDocument::Load
**Path**: `Projects/AMS/host/Ams.Core/Artifacts/TimingOverrides.cs`

## Summary
**Load a timing-overrides JSON file from disk into a strongly typed `TimingOverridesDocument` with explicit failure handling.**

`Load` is a synchronous file-backed factory that validates path existence, then deserializes JSON into `TimingOverridesDocument` using a shared `JsonSerializerOptions` configured for camelCase naming and indented output. It throws `FileNotFoundException` when the file is missing, reads the full file via `File.ReadAllText`, and calls `JsonSerializer.Deserialize<TimingOverridesDocument>(...)`. If deserialization returns null, it escalates with `InvalidOperationException` to guarantee a non-null document result.


#### [[TimingOverridesDocument.Load]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static TimingOverridesDocument Load(string path)
```


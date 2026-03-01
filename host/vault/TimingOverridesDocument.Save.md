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
  - llm/validation
  - llm/error-handling
---
# TimingOverridesDocument::Save
**Path**: `Projects/AMS/host/Ams.Core/Artifacts/TimingOverrides.cs`

## Summary
**Validate, normalize, and write a timing-overrides document to a JSON file at the specified path.**

`Save` persists the current `TimingOverridesDocument` to disk as JSON after enforcing basic invariants and deterministic ordering. It rejects empty payloads (`Sentences.Count == 0`) with `InvalidOperationException`, ensures the target directory exists (`Directory.CreateDirectory`), sorts sentence overrides by `SentenceId`, and serializes a copied document (`this with { Sentences = ordered }`) using shared `JsonOptions`. It writes the full JSON text with `File.WriteAllText(path, json)`.


#### [[TimingOverridesDocument.Save]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Save(string path)
```


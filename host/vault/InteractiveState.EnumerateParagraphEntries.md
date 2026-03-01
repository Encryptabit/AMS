---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/utility
---
# InteractiveState::EnumerateParagraphEntries
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Enumerate all scope entries in the interactive state whose kind is `Paragraph`.**

`InteractiveState.EnumerateParagraphEntries()` is an expression-bodied wrapper around `_entries.Where(entry => entry.Kind == ScopeEntryKind.Paragraph)`. It returns a lazily evaluated `IEnumerable<ScopeEntry>` over the internal `_entries` list, so filtering occurs during enumeration rather than upfront materialization. The method performs no null checks or copying and simply projects the current in-memory state by enum kind.


#### [[InteractiveState.EnumerateParagraphEntries]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IEnumerable<ValidateTimingSession.ScopeEntry> EnumerateParagraphEntries()
```


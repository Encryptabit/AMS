---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# InteractiveState::BuildParagraphLabel
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Formats and escapes a paragraph display label from paragraph metadata and computed sentence count.**

This private helper builds the paragraph tree label with a `StringBuilder`: it appends `"Paragraph {ParagraphId}"`, optionally adds `" [{info.Kind}]"` when `info.Kind` is non-null/non-whitespace, then appends `" ({count} sentences)"` using `GetParagraphSentenceCount(paragraph.ParagraphId)`. It returns `Markup.Escape(builder.ToString())`, so the rendered label is sanitized for console markup.


#### [[InteractiveState.BuildParagraphLabel]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private string BuildParagraphLabel(ParagraphPauseMap paragraph, ValidateTimingSession.ParagraphInfo info)
```

**Calls ->**
- [[InteractiveState.GetParagraphSentenceCount]]

**Called-by <-**
- [[InteractiveState.AppendParagraph]]


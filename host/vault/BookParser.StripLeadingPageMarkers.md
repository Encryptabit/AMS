---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookParser.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# BookParser::StripLeadingPageMarkers
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookParser.cs`

## Summary
**Strips leading page-marker artifacts from extracted text and normalizes left whitespace.**

`StripLeadingPageMarkers` removes page-number/header-like prefixes from a text fragment using a precompiled regex. It returns input unchanged for null/empty strings, otherwise applies `LeadingPageMarkerRegex.Replace(text, string.Empty, 1)` to strip at most one leading marker sequence and then `TrimStart()` to normalize remaining whitespace. This is a focused cleanup step used during PDF sentence normalization.


#### [[BookParser.StripLeadingPageMarkers]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string StripLeadingPageMarkers(string text)
```

**Called-by <-**
- [[BookParser.ParsePdfAsync]]


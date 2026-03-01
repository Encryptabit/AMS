---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookParser.cs"
access_modifier: "private"
complexity: 9
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# BookParser::ShouldIgnoreSuppressEntry
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookParser.cs`

## Summary
**Filters suppression candidates by rejecting blank and numeric/date-like metadata values that should not be used for text-edge suppression.**

`ShouldIgnoreSuppressEntry` decides whether a metadata value should be excluded from the suppression list as non-lexical noise. It immediately ignores null/whitespace values, then scans for any alphabetic character; entries containing letters are kept (`false`). For non-letter entries, it treats values composed only of digits, whitespace, and common separators (`:`, `-`, `/`, `.`) as ignorable (`true`), while any other character makes the entry non-ignorable (`false`).


#### [[BookParser.ShouldIgnoreSuppressEntry]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool ShouldIgnoreSuppressEntry(string value)
```

**Called-by <-**
- [[BookParser.ParseDocxAsync]]


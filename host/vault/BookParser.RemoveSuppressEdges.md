---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookParser.cs"
access_modifier: "private"
complexity: 14
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# BookParser::RemoveSuppressEdges
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookParser.cs`

## Summary
**Cleans sentence text by repeatedly removing configured suppress strings from edges and dropping residual pure-suppress content.**

`RemoveSuppressEdges` iteratively strips metadata-like suppress entries from the start/end of a sentence to remove echoed title/author text. After early return for blank input or empty suppress set, it trims and repeatedly scans `suppressList`: removing case-insensitive prefix/suffix matches, collapsing to empty when an entry fully contains the current working string, and looping until no changes occur. It then performs a final normalized membership check and returns empty if the remaining text exactly matches a suppress entry. The method is tolerant of blank suppress entries and preserves unmatched interior content.


#### [[BookParser.RemoveSuppressEdges]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string RemoveSuppressEdges(string sentence, HashSet<string> suppressList)
```

**Called-by <-**
- [[BookParser.ParseDocxAsync]]
- [[BookParser.ParsePdfAsync]]


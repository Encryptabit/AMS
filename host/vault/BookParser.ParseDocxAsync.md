---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookParser.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/async
  - llm/data-access
  - llm/factory
  - llm/error-handling
---
# BookParser::ParseDocxAsync
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookParser.cs`

## Summary
**Asynchronously parses a DOCX into structured book content and metadata with paragraph-level extraction and cleanup.**

`ParseDocxAsync` runs DOCX parsing inside `Task.Run` to avoid blocking and loads the document with `DocX.Load(filePath)`. It extracts core properties (title/author/other metadata), builds a suppression set via the local `AddSuppress` helper, iterates body paragraphs (filtered by package-part URI), strips metadata echoes with `RemoveSuppressEdges`, infers paragraph kind from style, and appends cleaned text plus `ParsedParagraph` entries while honoring `cancellationToken`. It returns a `BookParseResult` containing assembled text, optional metadata, and parsed paragraphs; any failure is caught and rethrown as `BookParseException`.


#### [[BookParser.ParseDocxAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<BookParseResult> ParseDocxAsync(string filePath, CancellationToken cancellationToken)
```

**Calls ->**
- [[AddSuppress]]
- [[BookParser.RemoveSuppressEdges]]
- [[BookParser.ShouldIgnoreSuppressEntry]]

**Called-by <-**
- [[BookParser.ParseAsync]]


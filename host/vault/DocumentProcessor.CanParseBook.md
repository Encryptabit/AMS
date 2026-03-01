---
namespace: "Ams.Core.Processors.DocumentProcessor"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/DocumentProcessor/DocumentProcessor.Indexing.cs"
access_modifier: "public"
complexity: 1
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# DocumentProcessor::CanParseBook
**Path**: `Projects/AMS/host/Ams.Core/Processors/DocumentProcessor/DocumentProcessor.Indexing.cs`

## Summary
**Determines whether the document parser can handle the supplied source file.**

`CanParseBook` is a lightweight capability check that instantiates `BookParser` and delegates directly to `parser.CanParse(sourceFile)`. It contains no additional normalization, exception handling, or extension logic beyond what `BookParser` implements.


#### [[DocumentProcessor.CanParseBook]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static bool CanParseBook(string sourceFile)
```

**Calls ->**
- [[BookParser.CanParse]]

**Called-by <-**
- [[BuildIndexCommand.BuildBookIndexAsync]]
- [[BookParsingTests.BookParser_CanParse_Extensions]]


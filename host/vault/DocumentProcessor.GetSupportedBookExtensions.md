---
namespace: "Ams.Core.Processors.DocumentProcessor"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/DocumentProcessor/DocumentProcessor.Indexing.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# DocumentProcessor::GetSupportedBookExtensions
**Path**: `Projects/AMS/host/Ams.Core/Processors/DocumentProcessor/DocumentProcessor.Indexing.cs`

## Summary
**Exposes the set of file extensions currently supported by the book parser.**

`GetSupportedBookExtensions` creates a new `BookParser` instance and returns its `SupportedExtensions` collection. The method is a direct pass-through with no caching, filtering, or transformation of extension values.


#### [[DocumentProcessor.GetSupportedBookExtensions]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IReadOnlyCollection<string> GetSupportedBookExtensions()
```

**Called-by <-**
- [[BuildIndexCommand.BuildBookIndexAsync]]


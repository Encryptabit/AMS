---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookParser.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "Ams.Core.Runtime.Book.IBookParser"
member_count: 15
dependency_count: 0
pattern: ~
tags:
  - class
---

# BookParser

> Class in `Ams.Core.Runtime.Book`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookParser.cs`

**Implements**:
- [[IBookParser]]

## Properties
- `SupportedExtensions`: IReadOnlyCollection<string>
- `_supportedExtensions`: HashSet<string>
- `_paragraphBreakRegex`: Regex
- `PdfSentenceSplitRegex`: Regex
- `MetadataBreakRegex`: Regex
- `LeadingPageMarkerRegex`: Regex
- `PdfInitLock`: object
- `_pdfiumInitialized`: bool

## Members
- [[BookParser.CanParse]]
- [[BookParser.ParseAsync]]
- [[BookParser.EnsurePdfiumInitialized]]
- [[BookParser.ParseDocxAsync]]
- [[BookParser.ParseTextAsync]]
- [[BookParser.ParseMarkdownAsync]]
- [[BookParser.ParseRtfAsync]]
- [[BookParser.ParsePdfAsync]]
- [[BookParser.TryGetPdfMetaText]]
- [[BookParser.SanitizePdfText]]
- [[BookParser.StripLeadingPageMarkers]]
- [[BookParser.RemoveSuppressEdges]]
- [[BookParser.ShouldIgnoreSuppressEntry]]
- [[BookParser.SplitPdfSentences]]
- [[BookParser.SplitOnMetadataBreaks]]


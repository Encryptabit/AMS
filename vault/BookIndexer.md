---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "Ams.Core.Runtime.Book.IBookIndexer"
member_count: 29
dependency_count: 1
pattern: ~
tags:
  - class
---

# BookIndexer

> Class in `Ams.Core.Runtime.Documents`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

**Implements**:
- [[IBookIndexer]]

## Dependencies
- [[Ams.Core.Runtime.Book.IPronunciationProvider_]] (`pronunciationProvider`)

## Properties
- `_blankLineSplit`: Regex
- `ForcedHyphenBreakRegex`: Regex
- `OcrChapterHeaderRegex`: Regex
- `_pronunciationProvider`: IPronunciationProvider
- `ChapterDuplicateRegex`: Regex
- `SectionTitleRegex`: Regex
- `NumberedHeadingRegex`: Regex

## Members
- [[BookIndexer..ctor]]
- [[BookIndexer.CreateIndexAsync]]
- [[BookIndexer.Process]]
- [[BookIndexer.GetHeadingLevel]]
- [[BookIndexer.ClassifySectionKind]]
- [[BookIndexer.ShouldStartSection]]
- [[BookIndexer.LooksLikeHeadingStyle]]
- [[BookIndexer.IsNonSectionParagraphStyle]]
- [[BookIndexer.LooksLikeStandaloneTitle]]
- [[BookIndexer.LooksLikeTableOfContentsEntry]]
- [[BookIndexer.LooksLikeSectionHeading]]
- [[BookIndexer.ApplyChapterDuplicateSuffixes]]
- [[BookIndexer.TokenizeByWhitespace]]
- [[BookIndexer.NormalizeTokenSurface]]
- [[BookIndexer.NormalizeHeadingArtifacts]]
- [[BookIndexer.TrimOuterQuotes]]
- [[BookIndexer.IsQuoteChar]]
- [[BookIndexer.IsSentenceTerminal]]
- [[BookIndexer.BuildParagraphTexts]]
- [[BookIndexer.NormalizeParagraphText]]
- [[BookIndexer.FoldAdjacentHeadings]]
- [[BookIndexer.CombineHeadingTitles]]
- [[BookIndexer.CollectLexicalTokens]]
- [[BookIndexer.ContainsLexicalContent]]
- [[BookIndexer.ShouldSkipParagraphFromIndex]]
- [[BookIndexer.ComputeFileHash]]
- [[BookIndexer.MyRegex]]
- [[BookIndexer.MyRegex1]]
- [[BookIndexer.MyRegex2]]


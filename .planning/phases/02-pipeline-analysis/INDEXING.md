# Indexing Processes

This document clarifies the two distinct indexing processes in the AMS pipeline: **Book Indexing** and **ASR Response Indexing**, explaining their purposes, timing, and relationship.

## Overview: Two Indexing Processes

The pipeline uses two separate indexing operations:

| Indexing Process | When | Purpose |
|------------------|------|---------|
| **Book Indexing** | Before ASR | Parse book structure into searchable words/sentences/sections |
| **ASR Response Indexing** | After ASR | Build token views for anchor matching and alignment |

These are **not** the same operation. Book indexing creates a static structural index from the book text. ASR response indexing creates runtime views for alignment algorithms.

## Part 1: Book Indexing

### Purpose

Book indexing transforms a book markdown file into a structured `BookIndex` that:
- Provides positional lookup for every word
- Defines sentence and paragraph boundaries
- Identifies chapter/section boundaries
- Enables chapter-level processing without reparsing

### When It Runs

Book indexing runs **first**, before any chapter processing:

```
PipelineService.RunChapterAsync()
├── EnsureBookIndexAsync()        ← Book indexing happens here
│   └── BookIndexer.CreateIndexAsync()
├── workspace.OpenChapter()
├── GenerateTranscript (ASR)      ← After book index exists
├── ComputeAnchors
└── ...
```

**Triggered when:**
- Book index file doesn't exist, OR
- `Force` or `ForceIndex` option is set

### Implementation

**Class:** `BookIndexer` (`host/Ams.Core/Runtime/Documents/BookIndexer.cs`)

**Method:** `CreateIndexAsync(BookParseResult, sourceFile, options, ct)`

### Input

| Input | Type | Description |
|-------|------|-------------|
| `parseResult` | `BookParseResult` | Parsed markdown with paragraphs, title, author |
| `sourceFile` | `string` | Path to original book file (for hash) |
| `options` | `BookIndexOptions?` | Average WPM for duration estimation |

### Processing Steps

```
BookIndexer.CreateIndexAsync()
├── BuildParagraphTexts(parseResult)     # Extract paragraph text/style/kind
├── FoldAdjacentHeadings()               # Merge consecutive headings
├── CollectLexicalTokens()               # Gather unique words for pronunciation
├── GetPronunciationsAsync()             # Optional G2P lookup
└── Process()
    ├── For each paragraph:
    │   ├── Check if section heading → start new SectionRange
    │   ├── TokenizeByWhitespace()
    │   ├── For each token:
    │   │   ├── Add BookWord(Text, WordIndex, SentenceIndex, ...)
    │   │   ├── Attach phonemes if available
    │   │   └── Check IsSentenceTerminal() → close sentence
    │   └── Close paragraph range
    ├── Close final section
    └── Return BookIndex
```

### Output

```csharp
record BookIndex(
    string SourceFile,           // Original book path
    string SourceFileHash,       // SHA256 for cache invalidation
    DateTime IndexedAt,          // Timestamp
    BookTotals Totals,           // Counts and estimated duration
    BookWord[] Words,            // Every word with position
    SentenceRange[] Sentences,   // Sentence boundaries
    ParagraphRange[] Paragraphs, // Paragraph boundaries
    SectionRange[] Sections      // Chapter/section boundaries
);
```

**Key output: `Words[]` array**

Each word gets a global index (0-based) that becomes the primary key for alignment:

```csharp
record BookWord(
    string Text,         // "Chapter"
    int WordIndex,       // 0, 1, 2, 3...
    int SentenceIndex,   // Which sentence
    int ParagraphIndex,  // Which paragraph
    int SectionIndex,    // Which chapter (-1 if none)
    string[]? Phonemes   // Optional G2P phonemes
);
```

### Section Detection

Book indexing identifies chapter boundaries using:
- **Style-based**: "Heading 1", "Chapter" styles
- **Text-based**: "Chapter", "Prologue", "Epilogue" keywords
- **Pattern-based**: Numbered headings like "1 - Title"

This enables per-chapter processing without reparsing the entire book.

---

## Part 2: ASR Response Indexing

### Purpose

ASR response indexing is **not a separate stage** but a preprocessing step within alignment that:
- Builds "anchor views" of ASR tokens for matching
- Filters out stopwords and non-content tokens
- Creates mappings between filtered and original token positions
- Enables phoneme-aware matching

### When It Runs

ASR response indexing happens **inside** the alignment stages:

```
ComputeAnchorsCommand.ExecuteAsync()
└── AlignmentService.ComputeAnchorsAsync()
    └── AnchorPreprocessor.BuildAsrView(asr)   ← ASR indexing here

BuildTranscriptIndexCommand.ExecuteAsync()
└── AlignmentService.BuildTranscriptIndexAsync()
    └── AnchorPreprocessor.BuildAsrView(asr)   ← And here
```

### Implementation

**Class:** `AnchorPreprocessor` (inferred from usage)

**Method:** `BuildAsrView(AsrResponse asr)`

### Input

| Input | Type | Description |
|-------|------|-------------|
| `asr` | `AsrResponse` | ASR output with tokens and timing |

### Processing Steps

```
AnchorPreprocessor.BuildAsrView(asr)
├── Extract tokens from asr.Tokens
├── Normalize each token (lowercase, strip punctuation)
├── Filter out stopwords
├── Build FilteredToOriginalToken[] mapping
└── Return AsrAnchorView
```

### Output

```csharp
// Conceptual structure (not a direct record)
AsrAnchorView {
    Tokens: string[]                    // Filtered, normalized tokens
    FilteredToOriginalToken: int[]      // Maps filtered[i] → original[j]
}
```

### Filtered vs Original Mapping

The key output is the mapping between filtered and original positions:

```
Original ASR:  "the", "quick", "brown", "fox", "uh", "jumps"
                 0      1        2       3      4       5
                 ↓      ↓        ↓       ↓      ×       ↓
Filtered:      "the", "quick", "brown", "fox",       "jumps"
                 0      1        2       3             4

FilteredToOriginalToken: [0, 1, 2, 3, 5]
```

This mapping is essential for:
- Anchor matching (work with filtered tokens)
- Recovering original positions (for timing lookup)

---

## Relationship Between Both Indexing Processes

### Data Flow

```mermaid
graph LR
    subgraph Book Indexing
        BF[Book Markdown] --> BI[BookIndex]
        BI --> |words, sections| BW[BookWord Array]
    end

    subgraph ASR
        AF[Audio File] --> ASR[ASR Service]
        ASR --> AR[AsrResponse]
    end

    subgraph ASR Indexing
        AR --> |tokens| AV[AsrAnchorView]
        AV --> |filtered tokens| FT[Filtered Tokens]
    end

    subgraph Alignment
        BW --> |book words| AN[Anchor Matching]
        FT --> |asr tokens| AN
        AN --> |sync points| TX[TranscriptIndex]
    end
```

### Parallel Structure

Both indexing processes create parallel structures that alignment matches:

| Book Indexing | ASR Indexing |
|---------------|--------------|
| `BookWord[]` with `WordIndex` | `AsrToken[]` with position |
| `BookView.Tokens[]` (filtered) | `AsrView.Tokens[]` (filtered) |
| `FilteredToOriginalWord[]` | `FilteredToOriginalToken[]` |

### Alignment Matching

The anchor matching algorithm (`AnchorPipeline.ComputeAnchors`) works with both views:

```
For each potential anchor point:
    bookNGram = BookView.Tokens[i:i+n]      // Filtered book tokens
    asrNGram = AsrView.Tokens[j:j+n]        // Filtered ASR tokens

    if match(bookNGram, asrNGram):
        bookOriginal = FilteredToOriginalWord[i]    // Get original book index
        asrOriginal = FilteredToOriginalToken[j]    // Get original ASR index
        anchor = (bookOriginal, asrOriginal)
```

---

## Complete Indexing Timeline

```
1. Pipeline Start
   └── EnsureBookIndexAsync()
       └── BookIndexer.CreateIndexAsync()
           ├── Parse book markdown
           ├── Build words/sentences/paragraphs/sections
           └── Write book-index.json

2. Chapter Open
   └── Load BookIndex from book-index.json

3. ASR Stage
   └── GenerateTranscriptCommand.ExecuteAsync()
       └── Write asr.json (tokens with timing)

4. Anchors Stage
   └── ComputeAnchorsCommand.ExecuteAsync()
       ├── AnchorPreprocessor.BuildBookView(book)   // Book → anchor view
       ├── AnchorPreprocessor.BuildAsrView(asr)     // ASR → anchor view
       └── AnchorPipeline.ComputeAnchors()          // Match both views

5. Transcript Stage
   └── BuildTranscriptIndexCommand.ExecuteAsync()
       ├── AnchorPreprocessor.BuildBookView(book)   // Same views
       ├── AnchorPreprocessor.BuildAsrView(asr)
       └── TranscriptAligner.AlignWindows()         // DTW alignment
```

---

## Key Differences Summary

| Aspect | Book Indexing | ASR Response Indexing |
|--------|---------------|----------------------|
| **Purpose** | Parse book structure | Prepare tokens for alignment |
| **Timing** | Once, before all chapters | Per-stage, during alignment |
| **Persistence** | Saved to `book-index.json` | In-memory only |
| **Scope** | Entire book | Current ASR response |
| **Output** | `BookIndex` record | `AsrAnchorView` (internal) |
| **Reuse** | Across all chapters | Within single alignment call |

---

## Why Two Indexing Steps?

1. **Efficiency**: Book index is built once, reused for all chapters. ASR views are lightweight and discarded.

2. **Separation of concerns**: Book structure is static; ASR preprocessing adapts to each response.

3. **Filtering**: Both need filtered views for matching, but filters may differ (stopwords, normalization).

4. **Position recovery**: Both maintain filtered→original mappings for recovering source positions.

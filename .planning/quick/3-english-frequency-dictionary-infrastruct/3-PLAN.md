---
phase: quick-3
plan: 01
type: execute
wave: 1
depends_on: []
files_modified:
  - host/Ams.Core/Runtime/Book/EnglishFrequencyDictionary.cs
  - host/Ams.Core/Runtime/Book/BookModels.cs
  - host/Ams.Core/Runtime/Book/BookIndexer.cs
  - host/Ams.Core/Ams.Core.csproj
  - host/Ams.Core/Resources/english-frequency-50k.txt
  - host/Ams.Tests/BookIndexerProperNounTests.cs
autonomous: true
requirements: [FREQ-DICT, BRACKET-PHRASES, PROPER-NOUN-EXTRACTION]

must_haves:
  truths:
    - "BookIndexer extracts bracketed phrases like [Frozen Spire] as proper nouns"
    - "BookIndexer flags words not in the frequency dictionary as proper noun candidates"
    - "BookIndexer flags words with frequency rank >50000 as rare/proper noun candidates"
    - "Each SectionRange carries a ProperNouns string array for downstream ASR prompting"
    - "Frequency dictionary loads once as a static singleton from embedded resource"
  artifacts:
    - path: "host/Ams.Core/Runtime/Book/EnglishFrequencyDictionary.cs"
      provides: "Static singleton frequency lookup with O(1) rank queries"
      exports: ["EnglishFrequencyDictionary"]
    - path: "host/Ams.Core/Runtime/Book/BookModels.cs"
      provides: "SectionRange with ProperNouns property"
      contains: "ProperNouns"
    - path: "host/Ams.Core/Runtime/Book/BookIndexer.cs"
      provides: "Bracket phrase tracking + frequency filtering in Process loop"
    - path: "host/Ams.Core/Resources/english-frequency-50k.txt"
      provides: "Vendored frequency word list as embedded resource"
    - path: "host/Ams.Tests/BookIndexerProperNounTests.cs"
      provides: "Tests for bracket extraction and frequency filtering"
  key_links:
    - from: "host/Ams.Core/Runtime/Book/BookIndexer.cs"
      to: "EnglishFrequencyDictionary"
      via: "static method call in Process loop"
      pattern: "EnglishFrequencyDictionary\\.(GetRank|IsRare|TryGetRank)"
    - from: "host/Ams.Core/Runtime/Book/BookIndexer.cs"
      to: "SectionRange.ProperNouns"
      via: "populated during section close"
      pattern: "ProperNouns.*properNouns"
---

<objective>
Add English word frequency dictionary infrastructure and proper noun extraction to BookIndexer.

Purpose: Enable ASR prompt biasing (Task 2) by identifying proper nouns, fantasy terms, and rare words per section during book indexing. This is the data-production step that feeds into WithPrompt() wiring later.

Output: EnglishFrequencyDictionary static class, updated SectionRange with ProperNouns, modified BookIndexer.Process with bracket phrase tracking and frequency filtering, plus tests.
</objective>

<execution_context>
@/home/cari/.claude/get-shit-done/workflows/execute-plan.md
@/home/cari/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@host/Ams.Core/Runtime/Book/BookIndexer.cs
@host/Ams.Core/Runtime/Book/BookModels.cs
@host/Ams.Core/Runtime/Book/PronunciationHelper.cs
@host/Ams.Core/Ams.Core.csproj
@host/Ams.Tests/Ams.Tests.csproj

<interfaces>
<!-- Key types and contracts the executor needs. -->

From host/Ams.Core/Runtime/Book/BookModels.cs:
```csharp
public record SectionRange(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("level")] int Level,
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("startWord")] int StartWord,
    [property: JsonPropertyName("endWord")] int EndWord,
    [property: JsonPropertyName("startParagraph")] int StartParagraph,
    [property: JsonPropertyName("endParagraph")] int EndParagraph
);
```

From host/Ams.Core/Runtime/Book/BookIndexer.cs (Process method token loop, lines 157-193):
```csharp
foreach (var rawToken in TokenizeByWhitespace(pText))
{
    var normalizedToken = NormalizeTokenSurface(rawToken);
    if (!ContainsLexicalContent(normalizedToken)) continue;
    // ... creates BookWord, checks sentence terminal
}
```

From host/Ams.Core/Runtime/Book/BookIndexer.cs (section close pattern, lines 128-142):
```csharp
if (currentSection != null)
{
    sections.Add(new SectionRange(
        Id: currentSection.Id,
        Title: currentSection.Title,
        Level: currentSection.Level,
        Kind: currentSection.Kind,
        StartWord: currentSection.StartWord,
        EndWord: endWord,
        StartParagraph: currentSection.StartParagraph,
        EndParagraph: endParagraph
    ));
}
```

From host/Ams.Core/Runtime/Book/PronunciationHelper.cs:
```csharp
public static string? NormalizeForLookup(string token);
// Returns lowercase, letters-only, with apostrophes preserved, hyphens split.
// E.g., "Herald's" -> "herald's", "Blood-sworn" splits into "blood" + "sworn"
```
</interfaces>
</context>

<tasks>

<task type="auto">
  <name>Task 1: Frequency dictionary infrastructure + SectionRange model update</name>
  <files>
    host/Ams.Core/Resources/english-frequency-50k.txt
    host/Ams.Core/Ams.Core.csproj
    host/Ams.Core/Runtime/Book/EnglishFrequencyDictionary.cs
    host/Ams.Core/Runtime/Book/BookModels.cs
  </files>
  <action>
**1a. Vendor the frequency word list.**

Create `host/Ams.Core/Resources/english-frequency-50k.txt` — a plain text file with one word per line, ordered by descending frequency (rank 1 = most common). Format: just the word, lowercase, one per line. No header. Generate this from common English corpora knowledge. Include the top 50,000 English words. The list should cover standard English vocabulary — common words like "the", "of", "and" at the top, progressing through less common but real English words. Words like "herald", "earthen", "gnoll", "voidling" should NOT be in this list (they are too rare or fantasy-specific). Words like "chapter", "prologue", "castle", "sword" SHOULD be present.

For practical purposes: generate approximately 50,000 lines. The exact source does not matter as long as common English words are well-represented and the ordering is roughly frequency-based. This is used as a filter, not a scoring mechanism — the key boundary is "in dictionary and rank <= 50000" vs "not in dictionary or rank > 50000".

**1b. Register as embedded resource in csproj.**

Add to `host/Ams.Core/Ams.Core.csproj`:
```xml
<ItemGroup>
  <EmbeddedResource Include="Resources\english-frequency-50k.txt" />
</ItemGroup>
```

**1c. Create `EnglishFrequencyDictionary.cs`.**

Location: `host/Ams.Core/Runtime/Book/EnglishFrequencyDictionary.cs`

Static class with lazy-loaded singleton dictionary. Implementation:
- Private `static readonly Lazy<FrozenDictionary<string, int>>` loaded from embedded resource via `Assembly.GetExecutingAssembly().GetManifestResourceStream(...)`.
- Resource name: `Ams.Core.Resources.english-frequency-50k.txt` (dotted namespace path).
- Parse: read lines, skip empty/whitespace, assign rank = line index + 1 (1-based). Key = line text trimmed and lowercased. Value = rank.
- Use `System.Collections.Frozen.FrozenDictionary` for O(1) lookup with zero ongoing allocation (.NET 8+, available on net9.0).

Public API:
```csharp
/// <summary>Returns the frequency rank (1 = most common) or -1 if not found.</summary>
public static int GetRank(string word)

/// <summary>True if the word is absent from the dictionary or has rank > rarityThreshold.</summary>
public static bool IsRareOrUnknown(string word, int rarityThreshold = 50_000)

/// <summary>Number of entries in the dictionary.</summary>
public static int Count { get; }
```

The `GetRank` method should lowercase the input before lookup. Accept already-lowered input efficiently (no allocation if already lower).

**1d. Add `ProperNouns` to `SectionRange`.**

Add an optional `ProperNouns` property to the `SectionRange` record in `BookModels.cs`:
```csharp
[property: JsonPropertyName("properNouns")]
string[]? ProperNouns = null
```

This must be the LAST parameter with a default value so existing `new SectionRange(...)` call sites remain valid without changes (they pass all positional args already; the new parameter with default `null` is simply omitted).
  </action>
  <verify>
    <automated>cd /home/cari/repos/AMS && dotnet build host/Ams.Core/Ams.Core.csproj --no-restore 2>&amp;1 | tail -5</automated>
  </verify>
  <done>
    - EnglishFrequencyDictionary.cs exists with GetRank/IsRareOrUnknown static methods
    - Frequency list embedded as resource in csproj
    - SectionRange has optional ProperNouns property
    - Ams.Core builds without errors
  </done>
</task>

<task type="auto" tdd="true">
  <name>Task 2: Bracket phrase tracking + frequency filtering in BookIndexer.Process</name>
  <files>
    host/Ams.Core/Runtime/Book/BookIndexer.cs
    host/Ams.Tests/BookIndexerProperNounTests.cs
  </files>
  <behavior>
    - Test: Bracketed text `[Frozen Spire]` in a section produces `"Frozen Spire"` in that section's ProperNouns
    - Test: Angle-bracketed text `<System Alert>` in a section produces `"System Alert"` in ProperNouns
    - Test: Nested/unclosed brackets with safety valve (>8 tokens) abandon accumulation, individual tokens fall through to frequency check
    - Test: Unknown word "Voidlings" (not in frequency dictionary) appears in ProperNouns
    - Test: Rare real English word with rank > 50000 (if any exist in the list) appears in ProperNouns
    - Test: Common English word "the" does NOT appear in ProperNouns
    - Test: Words inside brackets are NOT also individually frequency-checked (no double-hits)
    - Test: Multiple sections each get their own scoped ProperNouns arrays
    - Test: Hyphenated compound "Blood-sworn" where neither component passes frequency check yields entry in ProperNouns
  </behavior>
  <action>
**2a. Write tests first** in `host/Ams.Tests/BookIndexerProperNounTests.cs`.

Use the existing test pattern: create a temp `.txt` file with controlled content, parse via `DocumentProcessor.ParseBookAsync`, then run `BookIndexer.CreateIndexAsync`. Assert on the resulting `BookIndex.Sections[n].ProperNouns`.

For bracket tests, structure the text file with a heading line (to create a section) followed by body paragraphs containing bracketed terms.

For frequency tests, use known fantasy/invented words that will NOT be in any English frequency dictionary.

**2b. Modify `BookIndexer.Process`** to extract proper nouns during the existing token iteration loop.

Add per-section state tracking inside `Process`:
```csharp
var sectionProperNouns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
```

**Bracket phrase state machine** — add before the `foreach (var rawToken in TokenizeByWhitespace(pText))` loop body:
- Track `bool inBracket`, `List<string> bracketAccumulator`, `int bracketTokenCount`, `char expectedClosingBracket`.
- When `rawToken` starts with `[` or `<`: set `inBracket = true`, record expected closing char (`]` or `>`), start accumulating. Strip the opening bracket from the first token before adding to accumulator.
- While `inBracket`: add token text (stripped of brackets) to accumulator, increment count. If token ends with expected closing bracket or count > 6-8 (safety valve): close bracket phrase. On close: join accumulated tokens with space, add quoted phrase to `sectionProperNouns`. Reset state.
- While `inBracket`, SKIP the frequency check below (the `continue` or a flag prevents double-hit).
- Important: tokens inside brackets should still create `BookWord` entries and participate in sentence detection normally. Only the proper-noun frequency check is skipped.

**Frequency-based filtering** — for each non-bracketed lexical token:
- Extract a lowercase lookup form. Use the raw token surface (not the pronunciation-normalized form, which splits hyphens). Strip leading/trailing punctuation, apostrophe-possessive (`'s`, `'s`), lowercase the result.
- Call `EnglishFrequencyDictionary.IsRareOrUnknown(lookupForm)`. If true, add `rawToken` (original surface, trimmed of punctuation) to `sectionProperNouns`.
- For hyphenated tokens (contains `-`): split on `-`, check each component individually against the dictionary. If ALL components are rare/unknown, add the full hyphenated token.
- Skip tokens that are purely numeric or single characters.

**Wire ProperNouns into SectionRange** — when closing a section (the two existing `sections.Add(new SectionRange(...))` call sites in `Process`):
- Pass `ProperNouns: sectionProperNouns.Count > 0 ? sectionProperNouns.Order().ToArray() : null`.
- After closing a section, create a fresh `sectionProperNouns = new HashSet<string>(...)` for the next section.

**Vault improvements to apply along the way** (per the plan):
- `CollectLexicalTokens`: Change return type from `IEnumerable<string>` to `IReadOnlySet<string>`. The method already builds a `HashSet<string>` internally — just return it directly. Update the caller in `CreateIndexAsync` (the variable `lexicalTokens` type annotation, if any).
- `NormalizeTokenSurface`: The current implementation calls `TextNormalizer.NormalizeTypography(token).Trim()` then `TrimOuterQuotes`. Merge into a single pass: call `NormalizeTypography`, then do the trim + outer-quote strip in one scan from both ends (the `TrimOuterQuotes` method already does end-scanning; just inline it or combine the operations to avoid the intermediate string from `.Trim()`).
  </action>
  <verify>
    <automated>cd /home/cari/repos/AMS && dotnet test host/Ams.Tests/Ams.Tests.csproj --filter "FullyQualifiedName~BookIndexerProperNoun" --no-restore -v minimal 2>&amp;1 | tail -20</automated>
  </verify>
  <done>
    - All BookIndexerProperNounTests pass
    - Bracketed phrases extracted as quoted proper nouns per section
    - Unknown/rare words flagged as proper nouns per section
    - Common English words excluded from proper nouns
    - SectionRange.ProperNouns populated in BookIndex output
    - Existing BookParsing and Tokenizer tests still pass (no regressions)
    - CollectLexicalTokens returns IReadOnlySet
  </done>
</task>

</tasks>

<verification>
```bash
# Full test suite — no regressions
cd /home/cari/repos/AMS && dotnet test host/Ams.Tests/Ams.Tests.csproj --no-restore -v minimal

# Verify embedded resource is present in assembly
cd /home/cari/repos/AMS && dotnet build host/Ams.Core/Ams.Core.csproj && \
  dotnet script -e "var asm = System.Reflection.Assembly.LoadFrom(\"host/Ams.Core/bin/Debug/net9.0/Ams.Core.dll\"); foreach(var r in asm.GetManifestResourceNames()) Console.WriteLine(r);" 2>/dev/null || echo "Manual check: verify resource name in assembly"

# Verify SectionRange serialization includes properNouns when present
cd /home/cari/repos/AMS && dotnet build host/Ams.Core/Ams.Core.csproj
```
</verification>

<success_criteria>
- EnglishFrequencyDictionary loads ~50k words from embedded resource as FrozenDictionary
- SectionRange.ProperNouns is populated during BookIndexer.Process
- Bracket phrases ([...] and <...>) extracted as proper nouns
- Words not in frequency dictionary flagged as proper nouns
- All existing tests pass (no regressions)
- New proper noun tests pass
- Ready for Task 2 (WithPrompt wiring in AsrProcessor) to consume SectionRange.ProperNouns
</success_criteria>

<output>
After completion, create `.planning/quick/3-english-frequency-dictionary-infrastruct/3-SUMMARY.md`
</output>

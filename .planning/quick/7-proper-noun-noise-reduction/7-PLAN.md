# Quick-7: Proper Noun Noise Reduction

## Problem

`BookIndexer.CheckFrequencyForProperNoun` produces noisy ProperNouns arrays. Real book data (Apocalypse Healer 4) shows sections with 60-155 "proper nouns", the majority being false positives. Three categories of noise dominate:

### 1. Contractions (most common — appears in every section)
Tokens like `couldn't`, `didn't`, `shouldn't`, `he'd`, `I'll`, `I'm`, `aren't`, `won't`, `hasn't`, `we're`, `they've` etc.

**Root cause:** `ExtractLookupForm` strips possessive `'s` but doesn't handle contractions. `couldn't` → lookup form `couldn't` → not in dictionary → flagged as proper noun.

### 2. Em-dash compounds (second most common)
Tokens like `centuries—and`, `armor—were`, `before—though`, `alive—unless`, `Bloodthrone—undead`.

**Root cause:** The tokenizer doesn't split on `—` (U+2014) or `–` (U+2013). The whole compound fails dictionary lookup. Even if one side is a real proper noun, the compound form is garbage in the ProperNouns array.

### 3. Numeric/stat tokens
Tokens like `1487x`, `289x`, `0.01`, `2,352`, `100,000`, `50%—but`, `+50 Blood Droplets`.

**Root cause:** `ExtractLookupForm` only skips purely-digit tokens (`lookupForm.All(char.IsDigit)`). Tokens with `x`, `,`, `.`, `%`, `+` pass through.

### 4. Inflected common words (minor — reduced by SymSpell's larger dictionary)
Words like `grimaced`, `channeled`, `brandished`, `clattered`. These are inflected forms of common verbs that may not appear in the frequency dictionary.

**Not addressed here** — this would require stemming/lemmatization, which is heavier machinery. The SymSpell 82k dictionary already covers many inflected forms. Remaining cases are acceptable noise.

## Fix Location

All changes in `BookIndexer.CheckFrequencyForProperNoun` and `ExtractLookupForm` methods in `host/Ams.Core/Runtime/Book/BookIndexer.cs`.

## Tasks

### Task 1: Strip contractions before dictionary lookup (TDD)

**Files:** `BookIndexer.cs`, `BookIndexerProperNounTests.cs`

**Tests to add:**
- `Contraction_ShouldNt_NotInProperNouns` — "shouldn't" resolves to "should" → common → excluded
- `Contraction_HeD_NotInProperNouns` — "he'd" resolves to "he" → common → excluded
- `Contraction_WontAndCant_NotInProperNouns` — irregular contractions "won't" → "will", "can't" → "can"
- `Contraction_WithRareBase_StillDetected` — "Zyxorn's" already handled, but "Zyxorn'd" should also resolve to "Zyxorn" → rare → included

**Implementation in `ExtractLookupForm`:**

After the existing possessive `'s` strip, add contraction suffix stripping. Check for these suffixes (both `'` U+0027 and `'` U+2019):

| Suffix | Strip to base | Example |
|--------|--------------|---------|
| `n't` | remove suffix | `couldn't` → `could`, `shouldn't` → `should` |
| `'d` | remove suffix | `he'd` → `he`, `she'd` → `she` |
| `'ll` | remove suffix | `I'll` → `I`, `you'll` → `you` |
| `'m` | remove suffix | `I'm` → `I` |
| `'ve` | remove suffix | `I've` → `I`, `they've` → `they` |
| `'re` | remove suffix | `we're` → `we`, `they're` → `they` |

Special cases (handle before generic strip):
- `won't` → `will` (irregular)
- `can't` → `can` (irregular)

Order: possessive `'s` first (already exists), then contraction suffixes, then lowercase.

### Task 2: Split em-dash compounds and skip numeric tokens

**Files:** `BookIndexer.cs`, `BookIndexerProperNounTests.cs`

**Tests to add:**
- `EmDashCompound_BothCommon_NotInProperNouns` — "before—though" → both common → excluded
- `EmDashCompound_OneRare_OnlyRareSideAdded` — "Bloodthrone—undead" → "Bloodthrone" rare, "undead" common → only "Bloodthrone" added
- `NumericStatToken_NotInProperNouns` — "1487x", "2,352", "0.01", "50%" all excluded
- `NumericWithText_NotInProperNouns` — "+50" excluded

**Implementation in `CheckFrequencyForProperNoun`:**

**Em-dash handling** (before the existing hyphen check):
- If `rawToken` contains `—` (U+2014) or `–` (U+2013), split on those characters
- For each component, run through `ExtractLookupForm` → `IsRareOrUnknown` independently
- Add only the rare components as individual proper nouns (not the compound)
- Return early (don't fall through to single-token check)

**Numeric token skip** (replace existing digits-only check):
- Replace `lookupForm.All(char.IsDigit)` with a broader check:
- Skip if token matches pattern: starts with optional `+`/`-`, contains digits, and only other chars are `,`, `.`, `x`, `%`, `:`, `/`
- This catches: `1487x`, `2,352`, `0.01`, `50%`, `89D:23H:59M:59S`, `1/2`
- Simple implementation: after stripping, if all non-digit chars are in `{',', '.', 'x', '%', ':', '/', '+', '-'}`, skip

## Expected Impact

Based on the Apocalypse Healer 4 sample:
- ~40-60% reduction in ProperNouns entries per section (contractions + em-dash compounds are the bulk)
- Remaining entries should be predominantly real fantasy terms, character names, ability names, and LitRPG mechanics

## Verification

After implementation, re-index Apocalypse Healer 4 and spot-check 2-3 sections to confirm:
1. No contractions in ProperNouns
2. No em-dash compounds in ProperNouns
3. No numeric stat tokens in ProperNouns
4. Real proper nouns (Keros, Nihilum, Deryadus, Bloodthrone, Classers, etc.) still present

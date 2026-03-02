---
phase: quick-4
plan: 01
type: execute
wave: 1
depends_on: ["quick-3-01"]
files_modified:
  - host/Ams.Core/Processors/AsrProcessor.cs
  - host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs
autonomous: true
requirements: [ASR-PROMPT-WIRING]

must_haves:
  truths:
    - "AsrOptions carries an optional Prompt string for Whisper initial prompt"
    - "AsrProcessor.ConfigureBuilder calls builder.WithPrompt() when Prompt is non-empty"
    - "GenerateTranscriptCommand builds the prompt from the chapter's resolved SectionRange.ProperNouns"
    - "When no section or no proper nouns are available, ASR runs without a prompt (no crash)"
  artifacts:
    - path: "host/Ams.Core/Processors/AsrProcessor.cs"
      provides: "AsrOptions with Prompt property, ConfigureBuilder calls WithPrompt"
      contains: "WithPrompt"
    - path: "host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs"
      provides: "Prompt construction from BookIndex section ProperNouns"
      contains: "ProperNouns"
  key_links:
    - from: "host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs"
      to: "BookIndex.Sections"
      via: "chapter.Book.Documents.BookIndex lookup by Descriptor.BookStartWord"
      pattern: "BookIndex.*Sections.*ProperNouns"
    - from: "host/Ams.Core/Processors/AsrProcessor.cs"
      to: "WhisperProcessorBuilder.WithPrompt"
      via: "conditional call in ConfigureBuilder"
      pattern: "WithPrompt.*options\\.Prompt"
---

<objective>
Wire Whisper initial prompt support from BookIndex proper nouns through to the ASR processor.

Purpose: Whisper's `WithPrompt` biases decoding toward expected vocabulary. By feeding section-scoped proper nouns (fantasy names, rare words extracted by Task 3's BookIndexer changes), ASR accuracy improves for domain-specific content like audiobooks with invented terminology.

Output: Updated AsrOptions with Prompt field, ConfigureBuilder wiring, and GenerateTranscriptCommand prompt assembly from BookIndex sections.
</objective>

<execution_context>
@/home/cari/.claude/get-shit-done/workflows/execute-plan.md
@/home/cari/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@host/Ams.Core/Processors/AsrProcessor.cs
@host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs
@host/Ams.Core/Asr/AsrModels.cs
@host/Ams.Core/Services/AsrService.cs
@host/Ams.Core/Runtime/Chapter/ChapterContext.cs
@host/Ams.Core/Runtime/Book/BookContext.cs
@host/Ams.Core/Runtime/Book/BookModels.cs

<interfaces>
<!-- Key types and contracts the executor needs. -->

From host/Ams.Core/Processors/AsrProcessor.cs (AsrOptions record, line 636):
```csharp
public sealed record AsrOptions(
    string ModelPath,
    string Language = "auto",
    int Threads = 8,
    bool UseGpu = true,
    bool EnableWordTimestamps = true,
    bool SplitOnWord = true,
    int BeamSize = 5,
    int BestOf = 1,
    float Temperature = 0.0f,
    bool NoSpeechBoost = true,
    int GpuDevice = 0,
    bool UseFlashAttention = true,
    bool UseDtwTimestamps = false);
```

From host/Ams.Core/Processors/AsrProcessor.cs (ConfigureBuilder, lines 445-490):
```csharp
private static WhisperProcessorBuilder ConfigureBuilder(
    WhisperProcessorBuilder builder,
    AsrOptions options,
    bool enableTokenTimestamps)
{
    // ... existing builder configuration ...
    return builder;
}
```

From host/Ams.Core/Runtime/Book/BookModels.cs (SectionRange — after Task 3 adds ProperNouns):
```csharp
public record SectionRange(
    int Id, string Title, int Level, string Kind,
    int StartWord, int EndWord,
    int StartParagraph, int EndParagraph,
    string[]? ProperNouns = null   // <-- added by Task 3
);
```

From host/Ams.Core/Runtime/Chapter/ChapterContext.cs:
```csharp
public BookContext Book { get; }
public ChapterDescriptor Descriptor { get; }
// Descriptor has: BookStartWord, BookEndWord (nullable int?)
```

From host/Ams.Core/Runtime/Book/BookDocuments.cs:
```csharp
public BookIndex? BookIndex { get; set; }
```

From host/Ams.Core/Runtime/Book/BookModels.cs (BookIndex):
```csharp
public record BookIndex(
    ...,
    SectionRange[] Sections,
    ...);
```

Whisper.NET API (from NuGet XML docs):
```csharp
// WhisperProcessorBuilder
WhisperProcessorBuilder WithPrompt(string prompt);
// [EXPERIMENTAL] Configures the processor to use an initial prompt,
// which will be prepended to any existing text context.
```

From host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs:
```csharp
// RunWhisperAsync constructs AsrOptions and calls _asrService.TranscribeAsync(chapter, asrOptions, ct)
// chapter.Book.Documents.BookIndex is available at this point (loaded by PipelineService.EnsureBookIndexAsync)
// chapter.Descriptor.BookStartWord / BookEndWord identify which section this chapter maps to
```
</interfaces>
</context>

<tasks>

<task type="auto">
  <name>Task 1: Add Prompt to AsrOptions and wire WithPrompt in ConfigureBuilder</name>
  <files>host/Ams.Core/Processors/AsrProcessor.cs</files>
  <action>
**1a. Add `Prompt` parameter to the `AsrOptions` record.**

Add a new optional parameter at the end of the `AsrOptions` positional record (line 636-649). Place it after `UseDtwTimestamps`:

```csharp
public sealed record AsrOptions(
    string ModelPath,
    string Language = "auto",
    int Threads = 8,
    bool UseGpu = true,
    bool EnableWordTimestamps = true,
    bool SplitOnWord = true,
    int BeamSize = 5,
    int BestOf = 1,
    float Temperature = 0.0f,
    bool NoSpeechBoost = true,
    int GpuDevice = 0,
    bool UseFlashAttention = true,
    bool UseDtwTimestamps = false,
    string? Prompt = null);
```

Because it has a default value (`null`) and is the last parameter, all existing call sites remain valid without changes.

**1b. Wire `WithPrompt` in `ConfigureBuilder`.**

In the `ConfigureBuilder(WhisperProcessorBuilder builder, AsrOptions options, bool enableTokenTimestamps)` method (lines 445-490), add the prompt wiring after the beam/greedy strategy block (after line 487, before `return builder`):

```csharp
if (!string.IsNullOrWhiteSpace(options.Prompt))
{
    builder.WithPrompt(options.Prompt);
}
```

This is a simple conditional call. When Prompt is null or empty, no prompt is set and Whisper behaves as before.
  </action>
  <verify>
    <automated>cd /home/cari/repos/AMS && dotnet build host/Ams.Core/Ams.Core.csproj --no-restore 2>&amp;1 | tail -5</automated>
  </verify>
  <done>
    - AsrOptions has `string? Prompt = null` as final parameter
    - ConfigureBuilder calls `builder.WithPrompt(options.Prompt)` when non-empty
    - All existing code compiles without changes (default null preserves backward compat)
  </done>
</task>

<task type="auto">
  <name>Task 2: Build prompt from BookIndex ProperNouns in GenerateTranscriptCommand</name>
  <files>host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs</files>
  <action>
**2a. Add a static helper method to resolve the prompt string from the chapter's book context.**

Add this private static method to `GenerateTranscriptCommand`:

```csharp
private static string? BuildAsrPrompt(ChapterContext chapter)
{
    var bookIndex = chapter.Book.Documents.BookIndex;
    if (bookIndex?.Sections is not { Length: > 0 })
        return null;

    // Find the section matching this chapter via BookStartWord on the descriptor
    var startWord = chapter.Descriptor.BookStartWord;
    if (startWord is null)
        return null;

    SectionRange? section = null;
    foreach (var s in bookIndex.Sections)
    {
        if (s.StartWord == startWord.Value)
        {
            section = s;
            break;
        }
    }

    if (section?.ProperNouns is not { Length: > 0 })
        return null;

    // Join proper nouns with commas — Whisper treats the prompt as prior text context.
    // Comma-separated names prime the decoder vocabulary without forming sentences.
    var prompt = string.Join(", ", section.ProperNouns);
    return prompt.Length > 0 ? prompt : null;
}
```

The lookup uses `chapter.Descriptor.BookStartWord` which is set during `ChapterManager.CreateContext` from the section match. This is available before ASR runs because the BookIndex is loaded by `PipelineService.EnsureBookIndexAsync` and the chapter is opened with the resolved section info.

**2b. Wire the prompt into AsrOptions construction in `RunWhisperAsync`.**

In the `RunWhisperAsync` method, after the existing `AsrOptions` construction (around line 53-65), add the prompt:

Before the `var asrOptions = new AsrOptions(...)` call, resolve the prompt:
```csharp
var prompt = BuildAsrPrompt(chapter);
```

Then add the `Prompt` parameter to the `AsrOptions` constructor call:
```csharp
var asrOptions = new AsrOptions(
    ModelPath: modelPath,
    Language: options.Language,
    Threads: Math.Max(0, options.Threads),
    UseGpu: options.UseGpu,
    EnableWordTimestamps: options.EnableWordTimestamps,
    BeamSize: Math.Max(1, options.BeamSize),
    BestOf: Math.Max(1, options.BestOf),
    Temperature: (float)Math.Clamp(options.Temperature, 0.0, 1.0),
    NoSpeechBoost: true,
    GpuDevice: options.GpuDevice,
    UseFlashAttention: options.EnableFlashAttention,
    UseDtwTimestamps: options.EnableDtwTimestamps,
    Prompt: prompt);
```

**2c. Add a log line for observability.**

After constructing `asrOptions`, add a debug log so the user can verify the prompt is being applied:

```csharp
if (prompt is not null)
{
    Log.Debug("ASR prompt from BookIndex proper nouns ({Count} terms): {Prompt}",
        chapter.Book.Documents.BookIndex?.Sections?.FirstOrDefault(s => s.StartWord == chapter.Descriptor.BookStartWord)?.ProperNouns?.Length ?? 0,
        prompt.Length > 200 ? prompt[..200] + "..." : prompt);
}
```

Keep this simple — truncate long prompts in the log to avoid log spam. Use `Log.Debug` (the existing static Log helper used throughout this file, from Serilog).

**Important:** Do NOT add `using Ams.Core.Runtime.Book;` — it should already be transitively available via `ChapterContext`. If not needed, the `SectionRange` type comes via `Ams.Core.Runtime.Book` namespace. Add the using only if the compiler requires it.
  </action>
  <verify>
    <automated>cd /home/cari/repos/AMS && dotnet build host/Ams.Core/Ams.Core.csproj --no-restore 2>&amp;1 | tail -5</automated>
  </verify>
  <done>
    - BuildAsrPrompt resolves the chapter's section from BookIndex by matching BookStartWord
    - Prompt is comma-joined ProperNouns passed to AsrOptions
    - When no section or no ProperNouns exist, prompt is null and ASR runs as before
    - Debug log shows prompt content when applied
    - Full Ams.Core build succeeds
  </done>
</task>

</tasks>

<verification>
```bash
# Full build verification
cd /home/cari/repos/AMS && dotnet build host/Ams.Core/Ams.Core.csproj --no-restore

# Verify no regressions in test suite
cd /home/cari/repos/AMS && dotnet test host/Ams.Tests/Ams.Tests.csproj --no-restore -v minimal 2>&1 | tail -20

# Verify AsrOptions has Prompt parameter
grep -n "Prompt" host/Ams.Core/Processors/AsrProcessor.cs

# Verify WithPrompt call exists
grep -n "WithPrompt" host/Ams.Core/Processors/AsrProcessor.cs

# Verify BuildAsrPrompt exists and references ProperNouns
grep -n "ProperNouns\|BuildAsrPrompt" host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs
```
</verification>

<success_criteria>
- AsrOptions record has `string? Prompt = null` as its last parameter
- ConfigureBuilder conditionally calls `builder.WithPrompt(options.Prompt)` for non-empty prompts
- GenerateTranscriptCommand.RunWhisperAsync resolves proper nouns from BookIndex and passes them as the prompt
- Null/missing section or ProperNouns gracefully degrades to no prompt (existing behavior)
- All existing call sites compile without modification (backward compatible default)
- Full test suite passes
</success_criteria>

<output>
After completion, create `.planning/quick/4-wire-withprompt-in-asrprocessor/4-SUMMARY.md`
</output>

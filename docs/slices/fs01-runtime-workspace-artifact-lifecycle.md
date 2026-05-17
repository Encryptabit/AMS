# FS01: Runtime Workspace And Artifact Lifecycle

Last updated: 2026-05-17

Reader: an engineer refactoring AMS Core workspace, chapter, artifact, document slot, or runtime cache behavior.

Post-read action: preserve host/Core boundaries while making runtime artifact lifecycle states explicit and auditable.

## Built-In .NET Guard Inventory

AMS Core targets `net10.0`. Before writing a throwing contract, invariant, disposal, or cancellation check, check this inventory first. These are the public .NET `ThrowIf*` methods found in the .NET 10 reference surface.

Do not add custom `Guard`, `*Guard`, or `ThrowIf` helper classes/functions for ordinary constructor or method contracts or state invariants. The invariant should be visible where it is enforced. Use the built-in guard that directly matches the invariant. If no built-in guard matches, write an explicit local `if` and throw the standard exception at the boundary that owns the contract. Use validators, parsers, or result shapes for untrusted input and expected domain rejection; do not turn normal input errors into guard exceptions.

### Guards Versus Validators

Guards are for programmer errors, trusted-state corruption, impossible object states, and lifecycle misuse. Validators are for user choices, host configuration, CLI arguments, Workstation selections, external payloads, and contextual business policy that can be rejected during normal operation. A guard may throw; a validator should usually return a reportable result, issue list, or typed rejection.

In the current AMS app, the user-selected workspace path is an input boundary. Once a workspace has been accepted, chapter-open requests built from discovered workspace state are trusted runtime requests. Missing optional artifacts are ordinary absence, not request failure.

### Argument Guards

Use these for caller contract violations on method and constructor arguments. The signatures below include the `paramName` parameter so the overload is explicit. In normal AMS code, omit `paramName` and let the .NET `CallerArgumentExpression` feature capture the argument name. Pass `paramName` only when validating a transformed/local value but reporting the original public parameter.

| Invariant | Built-in guard |
|---|---|
| Reference argument must not be null | `ArgumentNullException.ThrowIfNull(argument, paramName = null)` |
| Pointer argument must not be null | `ArgumentNullException.ThrowIfNull(argument, paramName = null)` pointer overload |
| String argument must not be null or empty | `ArgumentException.ThrowIfNullOrEmpty(argument, paramName = null)` |
| String argument must not be null, empty, or whitespace | `ArgumentException.ThrowIfNullOrWhiteSpace(argument, paramName = null)` |
| Comparable argument must not equal a value | `ArgumentOutOfRangeException.ThrowIfEqual(value, other, paramName = null)` |
| Comparable argument must equal a value | `ArgumentOutOfRangeException.ThrowIfNotEqual(value, other, paramName = null)` |
| Comparable argument must be less than or equal to a maximum | `ArgumentOutOfRangeException.ThrowIfGreaterThan(value, other, paramName = null)` |
| Comparable argument must be less than a maximum | `ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(value, other, paramName = null)` |
| Comparable argument must be greater than or equal to a minimum | `ArgumentOutOfRangeException.ThrowIfLessThan(value, other, paramName = null)` |
| Comparable argument must be greater than a minimum | `ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, other, paramName = null)` |
| Numeric argument must be non-negative | `ArgumentOutOfRangeException.ThrowIfNegative(value, paramName = null)` |
| Numeric argument must be positive | `ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value, paramName = null)` |
| Numeric argument must be non-zero | `ArgumentOutOfRangeException.ThrowIfZero(value, paramName = null)` |

### State And Lifecycle Guards

Use these when the object or operation state owns the failure, not a caller argument range.

| Invariant | Built-in guard |
|---|---|
| Instance must not be disposed | `ObjectDisposedException.ThrowIf(condition, instance)` |
| Type-owned resource must not be disposed | `ObjectDisposedException.ThrowIf(condition, type)` |
| Operation must stop when cancellation is requested | `cancellationToken.ThrowIfCancellationRequested()` |

### Specialized BCL Guards

These are built into specific .NET APIs. Use them only when working directly with those API types.

| API invariant | Built-in guard |
|---|---|
| ASN.1 reader must have consumed all remaining data | `AsnReader.ThrowIfNotEmpty()` |
| Server-sent-event parser must be enumerated only once | `SseParser<T>.ThrowIfNotFirstEnumeration()` |

### When No Built-In Guard Exists

Some AMS invariants are real but have no matching .NET `ThrowIf*` helper. Keep those checks inline and explicit:

- invalid file names or path separators;
- non-finite numeric values such as `NaN` or infinity;
- cross-field rules such as `end > start`;
- collection-specific rules beyond null, empty, and count checks;
- domain membership rules such as known artifact kind, known module id, or valid chapter mapping.

Do not hide those checks behind a custom guard abstraction. The developer reading the function should see every invariant the function owns.

## Scope

FS01 owns runtime object lifetimes and host-agnostic access to books, chapters, audio buffers, and file-backed artifacts.

It does not own audio decode/resample policy, DSP treatment, or FFmpeg implementation details. Those belong to FS04.

## Current Concepts

- Workspaces give hosts a stable way to open chapters.
- Book, chapter, and audio managers control cached runtime contexts.
- File artifact resolution centralizes canonical paths.
- Document slots provide lazy artifact load/save behavior.
- Audio buffer descriptors identify available buffers for a chapter.

## Specific Changes Needed

- Replace loose chapter-opening option bags with a trusted Core request shape built after workspace validation.
- Move reusable artifact path construction into Core-owned artifact address values.
- Audit CLI and Workstation path-resolution one-offs and classify each as host policy or Core invariant.
- Keep hosts responsible for metadata and user choices, not loaded Core documents.
- Remove unearned nullable fields from runtime descriptors, including audio descriptor clip fields that do not model a real FS01 state.
- Keep decode, trim, resample, and FFmpeg policy out of FS01 descriptors; route that work to FS04.
- Make document slot lifecycle states explicit enough that save and invalidate behavior can be reviewed directly.
- Represent state transitions with their own invariants, not only with mutable flags or informal method order.
- Make cache limits and unload behavior visible for book, chapter, audio, and document lifetimes.
- Confirm behavior-oracle test coverage for the whole FS01 slice before reshaping callsites.

## Working Agreement

### Definition Of Done

FS01 is done when every type in the slice protects its own invariants, every callsite has been adjusted to the new shapes, and existing CLI/Workstation behavior is covered by tests before the refactor changes behavior.

That means:

- construction invariants are guarded at the type boundary;
- state transitions preserve invariants after every operation;
- nullable members have a written reason tied to a real lifecycle or absence state;
- host path-resolution one-offs have been classified as host policy or Core invariant;
- Core-owned resolution has one representation instead of repeated one-off string/path logic;
- cache/lifetime defaults are explicit and named, even if the first pass keeps current behavior;
- test coverage proves existing FS01 behavior remains intact.

### Required Order

1. Catalogue host path-resolution one-offs in CLI and Workstation.
2. Decide which of those one-offs become Core-owned concepts and which remain host policy.
3. Create a nullable policy for every FS01 type.
4. Confirm or add behavior-oracle tests for current FS01 behavior.
5. Introduce the new Core shapes and adjust callsites.
6. Make cache/lifetime policy explicit, starting with current defaults as named constants.
7. Add TTL plus LRU only where resource ownership justifies it.

### Cache And Lifetime Direction

Current keep-everything-loaded behavior can remain as the first-pass default if it is named and bounded by a clear policy object or constant. The immediate goal is to remove implicit behavior, not prematurely tune memory use.

The likely long-term policy is a combination of LRU and TTL:

- LRU protects memory when a large book touches many chapters.
- TTL protects long-running sessions from retaining stale, rarely used contexts forever.
- Audio buffers are the primary memory risk and should be easier to evict than lightweight metadata.
- File-backed documents may stay loaded once read if their lifecycle state and dirty behavior are explicit.

### State Transition Direction

Lifecycle states are not enough by themselves. Transitions also need invariant representation. A transition should say what state it accepts, what state it returns, and what invariant it preserves.

For document slots, examples include load, set clean, set dirty, write-through save, invalidate, save, and unload. For runtime caches, examples include create, touch, evict, deallocate, flush, and reload.

The combined philosophy already points at this through valid transitions, methods protecting transitions, and state transitions remaining auditable. FS01 should make it concrete.

## Discussion Map With Current Code

Use this section to drive the FS01 design discussion. Each item names the decision to make and the current code shape that forces the decision.

### 1. Definition Of Done

Current FS01 behavior is spread across workspace opening, chapter context creation, artifact resolution, document slots, and runtime caches. A reasonable FS01 completion bar should cover all of them.

Current code shape:

```csharp
public ChapterContextHandle OpenChapter(ChapterOpenOptions options)
{
    var bookIndex = options.BookIndexFile ?? ResolveDefaultBookIndex();
    var chapterDir = options.ChapterDirectory;

    if (chapterDir is null && options.ChapterId is { Length: > 0 })
    {
        chapterDir = new DirectoryInfo(Path.Combine(RootPath, options.ChapterId));
    }

    return Book.Chapters.CreateContext(...);
}
```

Agreed direction: FS01 is complete when every type protects its invariants, every callsite has been adjusted to the new shapes, and behavior-oracle tests prove existing CLI/Workstation behavior remains intact.

### 2. Host/Core Boundary

Workstation and CLI currently fill in defaults before calling Core. Some of that is host policy; some is reusable Core artifact resolution.

Current code shape:

```csharp
var bookIndex = options.BookIndexFile ?? ResolveDefaultBookIndex();
var chapterDir = options.ChapterDirectory;

if (chapterDir is null && options.ChapterId is { Length: > 0 })
{
    chapterDir = new DirectoryInfo(Path.Combine(RootPath, options.ChapterId));
}
```

Agreed direction: classify host path-resolution one-offs first. A host-selected workspace root and active chapter are host policy. Canonical artifact addresses and artifact naming rules should move toward Core.

### 3. Chapter Open Contract

`CreateContext` currently exposes a nullable parameter list. Required inputs are discovered inside the method rather than at the boundary.

Current code shape:

```csharp
public ChapterContextHandle CreateContext(
    FileInfo bookIndexFile,
    FileInfo? asrFile = null,
    FileInfo? transcriptFile = null,
    FileInfo? hydrateFile = null,
    FileInfo? audioFile = null,
    DirectoryInfo? chapterDirectory = null,
    string? chapterId = null,
    bool reloadBookIndex = false)
```

Agreed direction: replace the option bag with a trusted Core request shape after the host path-resolution audit and nullable policy are complete. User-facing validation belongs at workspace selection/opening; the chapter request should make required fields obvious and should reserve nullable values for real, named absence.

### 4. Artifact Address Model

Artifact paths are composed from root, chapter id, and suffix inside the resolver.

Current code shape:

```csharp
private static string GetChapterArtifactPath(ChapterContext context, string suffix)
{
    var directory = GetChapterRoot(context.Descriptor);
    var stem = GetChapterStem(context.Descriptor);
    return Path.Combine(directory, $"{stem}.{suffix}");
}
```

Agreed direction: introduce Core-owned artifact address values for reusable artifact construction.

Still open: decide whether FS01 needs separate book artifact and chapter artifact addresses, and whether known artifacts should be named values instead of loose suffix strings.

### 5. Failure Shapes

FS01 currently mixes argument guards, missing-file exceptions, nullable returns, and swallowed load failures.

Current code shape:

```csharp
ArgumentNullException.ThrowIfNull(bookIndexFile);
if (!bookIndexFile.Exists)
{
    throw new FileNotFoundException("Book index not found", bookIndexFile.FullName);
}
```

```csharp
if (!File.Exists(descriptor.Path))
{
    return null;
}
```

Agreed direction: choose the failure shape by meaning. Programmer contract violations should guard. Host/user validation should be reportable. Optional artifact absence can be nullable only when handled immediately. Malformed persisted AMS state should fail as corruption.

### 6. Document Slot Lifecycle

`DocumentSlot<T>` currently stores lifecycle as a boolean/null truth table.

Current code shape:

```csharp
private bool _loaded;
private bool _dirty;
private T? _value;

public T? GetValue()
{
    if (!_loaded)
    {
        var loaded = _loader();
        _value = loaded;
        _loaded = true;
    }

    return _value;
}
```

Agreed direction: crystallize lifecycle states and transitions. State types should protect allowed states; transition methods or values should protect allowed moves between those states.

### 7. Cache And Lifetime Policy

Chapter manager has an LRU-like cache with an implicit default. Audio manager has a cache without an explicit retained-buffer limit.

Current code shape:

```csharp
private const int DefaultMaxCachedContexts = int.MaxValue;

private void EnsureCapacity()
{
    while (_cache.Count > MaxCachedContexts && _usageOrder.First is { } lru)
    {
        ...
    }
}
```

Agreed direction: keep current defaults where acceptable, but make them named constants or policy values. Use LRU plus TTL as the likely robust direction, with audio buffers treated as the highest memory-cost cache entries.

### 8. Behavior Oracle

Before replacing resolution logic, FS01 needs tests that prove CLI and Workstation still open the same chapter and resolve the same artifacts.

Current code shape:

```csharp
var chapterStem = DetermineChapterStem(chapterId, audioFile, asrFile);
var chapterRoot =
    ResolveChapterRoot(chapterDirectory, audioFile, asrFile, bookIndexFile.Directory, chapterStem);
var audioPath = audioFile?.FullName ?? Path.Combine(chapterRoot, $"{chapterStem}.wav");
```

Agreed direction: confirm test coverage for the whole FS01 slice before refactor. Important oracle cases include explicit audio file, explicit chapter directory, missing optional artifacts, chapter id inferred from file names, and Workstation stem/title mapping.

### 9. Out Of Scope For FS01

The runtime audio descriptor currently carries decode and clip-adjacent fields, but Workstation slice behavior uses operation-local ranges and FS04 owns audio processing policy.

Current code shape:

```csharp
public sealed record AudioBufferDescriptor(
    string bufferId,
    string path,
    int? sampleRate = null,
    int? channels = null,
    TimeSpan? start = null,
    TimeSpan? duration = null)
```

Agreed direction: remove decode, resample, and clip policy from FS01 descriptor design. FS01 should identify the buffer artifact. FS04 should own decode options, slicing, trimming, resampling, and FFmpeg behavior.

## Decisions

### 2026-05-16 - Host Path-Resolution Audit Classification

CLI and Workstation keep host policy:

- selecting the workspace root;
- choosing the default book-index file when the user does not provide one;
- mapping a Workstation display title to a WAV stem;
- choosing a default chapter directory from the active workspace and selected chapter;
- accepting explicit user-provided audio, ASR, transcript, and hydrate files.

Core owns runtime invariants:

- validating that a chapter-open request has a real book-index file, chapter id, and chapter directory;
- inferring the chapter id from explicit audio/ASR file names when the host did not provide one;
- resolving the chapter root from explicit chapter directory, explicit artifact files, or book-index directory;
- constructing canonical chapter artifact addresses such as `{chapterId}.align.tx.json` and `{chapterId}.treated.wav`;
- preserving an explicit raw audio file as the raw buffer address while deriving Core-owned sibling artifact addresses from the chapter directory and chapter id.

### 2026-05-16 - FS01 Nullable Policy

Current retained nullables have named lifecycle meanings:

- `ChapterOpenOptions` remains a host input shape where null means "host/Core should apply defaults or no explicit override was selected."
- `ChapterOpenRequest` permits nullable artifact file overrides only for optional eager-load inputs; its book index, chapter id, and chapter directory are required.
- `DocumentSlot<T>` permits a loaded-null value only as `loaded-missing`, meaning the backing optional document was checked and no artifact exists.
- `BookStartWord` and `BookEndWord` remain nullable because a chapter may not map to a known book section.
- host current-chapter handles remain nullable because no current chapter may be selected.

Unearned audio descriptor nullables were removed from the FS01 runtime shape. Decode, resample, trim, and clip policy remain FS04-owned operation data instead of fields on every runtime buffer descriptor.

### 2026-05-16 - Current Cache And Lifetime Defaults Are Named

The first pass keeps current runtime retention behavior but names it:

- book contexts: retain all, save and release resources when explicitly deallocated;
- chapter contexts: retain known chapters by default, save and release audio resources on eviction/deallocation;
- audio buffers: retain loaded buffer contexts by default, release buffer resources on deallocation;
- document slots: keep file-backed documents loaded after read and save dirty values when the owning context saves.

TTL remains intentionally unset in this pass. LRU remains limited to the existing chapter-context eviction path until a later resource-ownership pass justifies broader eviction.

### 2026-05-16 - Hosts Provide Metadata; Core Owns Artifact Addresses

CLI and Workstation should not load files and pass already-loaded documents into Core as the normal path. Hosts should provide workspace metadata, selected chapter identity, explicit user overrides, and host-specific roots.

Core should convert that metadata into validated requests and canonical artifact addresses, then lazily load file-backed documents when the use case needs them.

### 2026-05-16 - Runtime Audio Descriptors Identify Buffers Only

An FS01 audio descriptor should identify a buffer and its Core-owned artifact address. It should not carry nullable clip ranges, decode settings, resample settings, or other FS04-owned audio processing policy.

The current `AudioBufferDescriptor.Start` and `AudioBufferDescriptor.Duration` members do not have a clear FS01 business reason. Workstation slice behavior uses operation-local ranges instead:

- playback region requests decode or trim from route/query range values;
- pickup replacement uses source references plus explicit start/end values;
- preview and audition clips trim loaded buffers from computed local ranges.

If clipped audio becomes a real reusable concept, model it as a named range or state-specific descriptor in the slice that owns that operation. Do not add nullable time fields to every runtime buffer descriptor.

### 2026-05-16 - Decode And Resample Policy Belongs To FS04

FS01 may know that a chapter has raw, treated, corrected, or filtered audio artifacts. It should not decide how those artifacts are decoded, resampled, trimmed, encoded, or treated.

Decode and resample policy belongs to FS04-owned operations such as audio processors, FFmpeg integration, treatment, splice, waveform, QC, and explicit audio-processing use cases.

### 2026-05-16 - Nullable Members Must Represent Real Lifecycle States

Nullable values should be rare. A nullable is acceptable in FS01 when absence is a real runtime state and the owning API names what absence means.

Examples that can be legitimate:

- an optional artifact is absent and the caller handles that immediately;
- a document slot has loaded and found no backing document;
- no current chapter has been selected in a host-owned workspace state.

Examples that should be challenged:

- nullable fields kept for possible future behavior;
- nullable descriptor properties that every caller must mentally interpret;
- option bags where required inputs are not clear until deep inside `CreateContext`.

### 2026-05-16 - Document Slot Lifecycle Should Be Explicit

Document slot behavior should be auditable. The current hidden state is a combination of loaded flag, dirty flag, and nullable value. FS01 cleanup should move toward named states so save and invalidate behavior can be reviewed without decoding a truth table.

## Code Sketches

### Audio Buffer Descriptor Target Shape

This sketch is FS01-only. It identifies a buffer and the artifact address Core will use. It does not model decode policy or clipped ranges.

```csharp
public sealed record AudioBufferDescriptor
{
    public AudioBufferDescriptor(
        string bufferId,
        ChapterArtifactAddress address)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bufferId);
        ArgumentNullException.ThrowIfNull(address);

        BufferId = bufferId;
        Address = address;
    }

    public string BufferId { get; }
    public ChapterArtifactAddress Address { get; }
}
```

### Artifact Address Target Shape

The exact implementation may change, but FS01 needs one Core-owned representation for chapter artifact addresses.

```csharp
public sealed record ChapterArtifactAddress
{
    public ChapterArtifactAddress(string chapterRoot, string chapterId, string suffix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterId);
        ArgumentException.ThrowIfNullOrWhiteSpace(suffix);

        var normalizedSuffix = suffix.Trim().TrimStart('.');
        ArgumentOutOfRangeException.ThrowIfEqual(normalizedSuffix.Length, 0, nameof(suffix));
        if (normalizedSuffix.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || normalizedSuffix.Contains(Path.DirectorySeparatorChar)
            || normalizedSuffix.Contains(Path.AltDirectorySeparatorChar))
        {
            throw new ArgumentException(
                "Artifact suffix cannot contain path separators or invalid file name characters.",
                nameof(suffix));
        }

        ChapterRoot = Path.GetFullPath(chapterRoot);
        ChapterId = chapterId;
        Suffix = normalizedSuffix;
    }

    public string ChapterRoot { get; }
    public string ChapterId { get; }
    public string Suffix { get; }

    public FileInfo ToFile() => new(Path.Combine(ChapterRoot, $"{ChapterId}.{Suffix}"));
}
```

### Document Slot State Target Shape

The important move is not this exact type hierarchy. The important move is that loaded/missing/dirty/invalidated state stops being implicit.

```csharp
internal abstract record DocumentSlotState<T>
    where T : class;

internal sealed record NotLoaded<T> : DocumentSlotState<T>
    where T : class;

internal sealed record LoadedMissing<T> : DocumentSlotState<T>
    where T : class;

internal sealed record LoadedClean<T>(T Value) : DocumentSlotState<T>
    where T : class;

internal sealed record LoadedDirty<T>(T Value) : DocumentSlotState<T>
    where T : class;

internal sealed record Invalidated<T>(bool PreserveDirty) : DocumentSlotState<T>
    where T : class;
```

## Open Audit Questions

- Which host path-resolution one-offs should become Core-owned artifact address rules?
- Which host path-resolution decisions are true host policy and should remain outside Core?
- What cache limits should be explicit for book, chapter, audio, and document lifetimes?
- Which current nullables represent real lifecycle states, and which are unearned optionality?
- What legacy behavior should become the oracle before replacing path resolution?

## Cross-Slice Boundaries

- FS04 owns decode, encode, trim, resample, treatment, splice, waveform extraction, QC, silence detection, and FFmpeg integration.
- FS05 owns alignment and timing artifact contracts. FS01 can resolve and load those artifacts but should not own their semantic rules.
- FS07 owns orchestration and pipeline entry points. FS01 should provide stable runtime access, not pipeline policy.

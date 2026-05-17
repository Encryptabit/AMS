# FS02: Book Ingestion, Indexing, And Pronunciation

Last updated: 2026-05-17

Reader: an engineer changing manuscript parsing, book indexing, cache behavior, or pronunciation enrichment.

Post-read action: record specific FS02 alignment work here before changing parser/indexer contracts, cache rules, pronunciation behavior, or book metadata shape.

Status: complete for the current cleanup pass as of 2026-05-17. Remaining notes in this file are future audit context unless they are explicitly reopened.

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

FS02 owns manuscript parsing, index construction, cached book metadata, pronunciation providers, and proper noun filtering.

## Current Concepts

- Manuscript parsing extracts source text from supported input formats.
- Book indexing builds canonical words, sentences, paragraphs, sections, and totals.
- Book cache stores parse/index results.
- Pronunciation providers enrich book/index data for alignment and MFA.

## Specific Changes Needed

- First pass: make the book-index cache boundary explicit. New book-index artifacts should carry a schema version, and cache reads should reject stale/corrupt/incompatible index shapes as normal validation misses rather than letting malformed cached metadata flow into runtime code.
- Keep indexer option invariants visible at the indexing boundary. `AverageWpm` must be finite and positive; invalid values are caller/configuration validation failures, not hidden guard helper logic.
- Remove the static `DocumentProcessor` compatibility facade and route callers directly to runtime book services (`BookParser`, `BookIndexer`, `BookCache`, `BookPhonemePopulator`) where the real behavior lives.
- Keep cache artifact pathing, JSON file access, source-file metadata checks, and source-file hashing in the runtime artifact layer. `BookCache` should own cache policy and compatibility decisions, not filesystem layout, metadata probing, or hashing implementations.
- Preserve manuscript text in `BookWord.Text`. Analysis-only repairs, such as forced hyphen line-break joining for pronunciation lookup, must not rewrite canonical word text.

## Decisions

- Cached `book-index.json` entries are external artifacts. Compatibility failures should return a cache miss and remove the stale cache file; they should not be treated as internal runtime corruption.
- Current runtime/workspace `book-index.json` loading remains permissive for existing workspace artifacts. FS02 cache compatibility is the first compatibility boundary; broader workspace artifact migration can be handled in a later slice if needed.
- Do not keep forwarding/static book-ingestion APIs around for compatibility when the call sites are cheap to update. FS02 should reduce indirection rather than preserve misleading abstractions.
- File hashing is shared artifact behavior. Book indexing and cache validation should read the lazy `Sha256Hash` property from a `FileArtifact<TAddress>` instead of each carrying local SHA256 code.
- `BookWord.Text` is the canonical manuscript surface. The indexer may build a separate analysis surface for pronunciation/proper-noun heuristics, but it must store the manuscript span unchanged.

## Code Sketches

No code sketches recorded yet.

## Open Audit Questions

- Which book-index invariants should move into constructors or factories?
- Which parser/indexer failure shapes are user validation versus internal corruption?
- Which cached book artifacts need compatibility/version handling before refactor?
- Should workspace `book-index.json` loading eventually report compatibility diagnostics through a validator/result shape before opening a book context?

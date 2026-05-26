# FS09: Validation And Reporting

Last updated: 2026-05-17

Reader: an engineer changing validation reports, script validation, report models, or hydrated/text diff scoring.

Post-read action: record specific FS09 alignment work here before changing validation failure shapes, report DTOs, or diff scoring behavior.

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

FS09 owns validation reports, script validation, hydrated/text diff scoring, and report view models.

## Current Concepts

- Script validation compares audio/ASR/book-derived data.
- Report builders transform validation results into report views.
- Text diff analysis computes scoring details and coverage.

## Specific Changes Needed

No slice-specific changes recorded yet.

## Decisions

No slice-specific decisions recorded yet.

## Code Sketches

No code sketches recorded yet.

## Open Audit Questions

- Which validation failures are user-facing findings versus internal errors?
- Which report DTOs need construction guards?
- Which diff-scoring behavior needs old-output oracle tests before cleanup?

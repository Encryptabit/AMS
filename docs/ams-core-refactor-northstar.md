# AMS Core Refactor Northstar

Last updated: 2026-05-16

Reader: an engineer refactoring AMS Core slice by slice.

Post-read action: choose refactor moves that make AMS Core more correct, explicit, and maintainable without importing patterns that do not fit the slice.

## Purpose

This document supplements the combined engineering philosophy with AMS-specific examples. It is the cross-slice standard for how cleanup should feel and how refactor decisions should be judged.

It is not the place for detailed slice decisions or long code sketches. Those belong in the slice alignment plans under `docs/slices/`.

Use the docs this way:

- the feature slice catalogue assigns every Core file and line to a stable slice;
- this northstar explains the philosophy and cross-slice review standard;
- each slice alignment plan records concrete changes, code sketches, decisions, boundary notes, and open audit questions.

## Practical Evidence

The production refactor that informed this effort showed several useful moves.

First, a multi-customer service lookup window became an explicit value. The value carried the customer id, start instant, and end instant together. Its constructor rejected invalid customer ids and inverted time ranges. The important part was not the customer domain; it was that the valid unit of work became impossible to partially assemble.

Second, the query object accepted a bounded collection of those values. It rejected null, empty, and over-limit batches before reaching SQL. The batch size was named as a constant near the query. That made the resource limit reviewable instead of tribal knowledge.

Third, the new batched path was tested against the old per-customer path. The old behavior was not thrown away as soon as the new implementation existed. It became an oracle for equivalence while the implementation changed.

Fourth, a timezone bug became a precise regression test. The test named the historical failure shape and asserted the exact offset-sensitive boundary that had failed. It did not only assert that the final output was non-empty.

Fifth, performance work did not hide the business rule. The batched query removed repeated I/O, but the mapping step still regrouped by owner before applying owner-specific calendar rules. The optimization respected the domain boundary.

## AMS Examples For The Combined Philosophy

These examples translate the combined philosophy into AMS vocabulary. They name the direction of travel. Detailed implementation sketches live in the owning slice docs.

### Priority Order

Safety and correctness beat convenience when Core opens a chapter, resolves an artifact, runs audio processing, or consumes persisted state. A host may supply partial metadata, but Core should not silently guess in a way that changes which chapter, artifact, or audio source is used.

Nullable values should be rare. A nullable is acceptable only when absence is a real domain state and the owning API names what absence means. Do not keep nullable properties for hypothetical future behavior.

### Core Belief

Put each rule where the runtime can help enforce it. If artifact paths are composed from loose strings in several places, make artifact address construction a Core-owned value. If a run artifact must always have a name, kind, path, and existence flag, keep those facts together and reject blank construction.

Use built-in .NET guard helpers when they express the invariant directly. Add a custom guard helper only when the same AMS rule repeats enough that naming it removes noise without hiding the checked value.

### The Two Pressures

Gradually pressure asks what domain concept is hidden behind a primitive, nullable, or option bag. In AMS, common candidates are chapter identity, artifact address, document slot state, run failure, benchmark compatibility, pause policy, and audio slice range.

TigerStyle pressure asks whether the result is bounded and auditable. A new value type that hides control flow, adds allocation in a hot loop, or spreads ceremony without removing an invalid state has not earned its place.

### Types Are For Rules, Not Decoration

Create a type when it prevents a real AMS mistake. Examples that can earn their keep:

- chapter identity, when it normalizes or rejects invalid identifiers;
- artifact address, when it prevents path/suffix confusion;
- document slot state, when it replaces a hidden boolean/null lifecycle table;
- run failure, when it distinguishes validation, dependency, timeout, cancellation, and execution failures;
- audio range, when it proves `end > start` for an operation that truly owns a clipped region.

Do not wrap every string. Wrap the strings that carry rules.

### Booleans Should Answer, Not Define State

A boolean method can answer a local question. A set of booleans and nullables that controls which operations are legal is usually a missing state model.

AMS examples:

- document slot lifecycle should be inspectable as not loaded, loaded missing, loaded clean, loaded dirty, or invalidated;
- benchmark readiness should not be a loose group of booleans when the UI and runner need to explain why execution is allowed or blocked;
- fit/preview acceptance should preserve enough state to reject stale preview acceptance deterministically.

### Contracts Belong At Boundaries

Hosts provide metadata and user choices. Core owns reusable contracts.

For FS01, that means CLI and Workstation can provide selected workspace, selected chapter, explicit user overrides, and host-specific roots. Core should convert those into validated requests and canonical artifact addresses, then lazily load file-backed documents when the use case needs them.

For FS04, that means decode, trim, resample, treatment, splice, and FFmpeg policy belongs to audio operations, not to runtime descriptors that merely identify available buffers.

### Invariants Are Not Comments

Comments can explain why a rule exists, but code must enforce it. AMS invariants should fail at construction or at the operation boundary that owns them.

Examples:

- blank artifact names and paths are invalid run artifacts;
- cache limits must be positive when configured;
- audio ranges must have finite `start` and `end` values and `end > start`;
- persisted AMS-owned JSON that cannot deserialize is corruption, not ordinary absence;
- host-provided chapter options should become a validated Core request before opening a chapter.

### Immutability Is An Encapsulation Tool

Use immutable records for small domain values: artifact addresses, open requests, run artifacts, run failures, benchmark policy snapshots, and compatibility results.

Do not push allocation-heavy immutable transitions into hot paths without evidence. Audio buffers, FFmpeg frames, waveform extraction, transcript tokenization, chunk loops, and large benchmark metrics aggregation need bounded mutation behind narrow methods.

### Keep Control Flow Visible

Prefer explicit, auditable control flow when the rule matters.

Audio precedence is a domain decision: corrected, then treated, then raw. Artifact precedence is a domain decision: explicit override, then chapter-local artifact, then book-level fallback if the slice allows it. These decisions should be readable without decoding a fluent chain or a strategy registry.

### Prefer The Smallest Useful Failure Shape

Use different failure shapes for different AMS meanings.

| AMS situation | Preferred shape | Meaning |
|---|---|---|
| Invalid argument to a Core API | built-in guard or argument exception | Caller broke the contract |
| Local optional artifact missing | nullable, handled immediately | Absence is ordinary and local |
| Normal lookup absence | `TryGet...` | Caller should branch on the next line |
| Host/user input rejected | validation result or small local `Result<T>` | Caller can report and recover |
| AMS-owned persisted state malformed | exception with context | Internal state is corrupt or incompatible |
| Pipeline operation failed | `RunFailure` at orchestration boundary | UI/CLI can report the operating failure |

Do not return `Result<T>` for impossible states. Do not throw for routine domain outcomes that callers are expected to handle.

### `TryCreate` And `FromTrusted`

Use `TryCreate` when a value crosses into AMS from an untrusted boundary: CLI args, Workstation input, external payloads, or tests modeling user input.

Use `FromTrusted` when rebuilding a value from AMS-owned state that should already satisfy the contract. If it fails, that is corruption or an earlier bug, not normal validation.

Both paths should preserve the same domain rule. They differ because the failure meaning is different.

### Monadic Thinking, Light Use

Use monadic thinking to identify repeated control-flow responsibilities, then choose the lightest C# expression.

Default AMS choices:

- nullable for local optional values handled immediately;
- `TryGet` when absence is normal and the caller should branch;
- a small local `Result<T>` only when validation failure must travel across a boundary;
- explicit `if` when a wrapper would force readers to translate back to ordinary control flow.

Do not add an Option/Result dependency just to make the code look more functional.

### Performance And Allocation Pressure

Immutable values fit descriptors, addresses, requests, run artifacts, run failures, and policy snapshots.

Pause before immutable allocation when the code owns buffers, external resources, hot loops, or latency-sensitive processing.

AMS-specific resource examples that should be explicit:

- maximum cached chapter contexts;
- maximum retained audio buffers per chapter;
- maximum MFA workspaces rented by pipeline concurrency;
- maximum ASR/MFA chunk count or chunk duration;
- maximum stale artifact scan breadth when resolving a chapter.

### Dependency And Tool Pressure

Use platform features first. Write tiny local helpers when the behavior is small. Add a dependency only when it materially improves safety or removes substantial complexity.

If a dependency becomes justified, write down why: what invalid state it prevents, what ceremony it removes, and why a local type is not enough.

### Practical Defaults For C#

Use these defaults until a slice proves it needs something else.

| AMS design choice | Default |
|---|---|
| Constructor and method contracts | built-in .NET guards |
| Repeated domain-specific guard noise | narrow guard helper with an exact name |
| Host/user input to Core | `TryCreate` or request validation |
| AMS-owned persisted state | `FromTrusted` plus guards |
| Local optional artifact | nullable only when absence is handled immediately |
| Normal lookup absence | `TryGet` |
| Cross-boundary validation rejection | small local `Result<T>` only if it removes repeated branching |
| Optional configuration mode | named state or overload before nullable properties |
| Descriptor or artifact address | immutable record |
| Audio buffer or FFmpeg-owned memory | bounded mutable owner |
| Artifact precedence | explicit `if` or named policy method |

## Slice Review Checklist

Before accepting a slice cleanup, answer:

1. What invalid state became impossible?
2. Which rule moved closer to the concept that owns it?
3. Which boundary now has a clearer contract?
4. What resource limit became visible?
5. What failure now has a more honest shape?
6. Which old behavior did the tests use as an oracle?
7. Which specific historical or plausible bug shape is now covered?
8. Did the code become easier to audit, or did the abstraction hide the decision?
9. Is the decision recorded in the owning slice doc?

If the answers are weak, the cleanup is probably cosmetic.

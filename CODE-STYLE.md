# AMS Code Style

This file is the durable engineering contract for AMS.

It exists so future work does not have to re-establish the same philosophy in every session.

AMS adopts TigerBeetle's style **in spirit and where practical**, adapted to the realities of the current stack:
- `.NET` / `C#`
- `Blazor Server` Workstation
- CLI host over shared Core/application services
- future `Zig` engine and DSP work

This is an AMS document, not a literal Zig transplant.
Read it against the repo that exists today: shared Core/application seams consumed by a CLI host and a Blazor Workstation host, with a future Zig track called out separately.

Priority order for engineering decisions:
1. **Safety**
2. **Performance**
3. **Developer experience**

If a tradeoff is real, preserve that ordering.

### Read this contract against current AMS seams

- `host/Ams.Core/Runtime/Workspace/IWorkspace.cs` is the shared typed workspace boundary.
- `host/Ams.Cli/Workspace/CliWorkspace.cs` and `host/Ams.Workstation.Server/Services/BlazorWorkspace.cs` are host adapters over that boundary; host-specific defaults and persisted state may live there, but business rules should not drift outward just because a host needs them.
- `host/Ams.Core/Services/PipelineService.cs` and `host/Ams.Core/Application/Validation/ValidationService.cs` are the current examples of orchestration and application logic living in shared layers instead of inside host commands, pages, or controllers.
- `host/Ams.Cli/Program.cs` and `host/Ams.Workstation.Server/Program.cs` are composition roots. They should register and wire services, not become the primary home of domain logic.
- Where the repo is not fully there yet, treat that gap as evidence for later audit/refactor slices, not as a reason to weaken the contract.

---

## 1. Adopt Directly

These principles are binding across the project now.

### 1.1 Explicit control flow
- Prefer simple, explicit control flow.
- Avoid hidden orchestration, surprising callbacks, and implicit background behavior.
- Keep branching decisions near the coordinating function.
- Push non-branching work into helpers.

### 1.2 Put a limit on everything
- Queue sizes, concurrency, retry counts, scan breadth, cache sizes, batch sizes, retained history, and long-running work must have explicit limits.
- If something is intentionally unbounded, document why and assert the expected execution pattern where possible.

### 1.3 Assertions are mandatory
- Assertions are required at important preconditions, postconditions, invariants, and state transitions.
- Prefer split assertions over compound assertions.
- Assert both the positive space you expect and the negative space you do not expect.
- For architectural work, assertions are not optional documentation; they are part of the design.

### 1.4 Differentiate programmer errors from operating errors
- Programmer errors should fail fast.
- Expected operating failures must be handled explicitly and surfaced clearly.
- Do not swallow important failures behind vague messages or fallback behavior.

### 1.5 Typed contracts over implicit behavior
- Important boundaries should be represented with typed contracts where practical.
- CLI flags and UI options are adapters over typed internal contracts, not the canonical model themselves.
- Avoid hidden defaults in correctness-critical paths.
- In AMS today, this includes seams like `IWorkspace` for shared chapter access and typed options flowing into services like `PipelineService` and `ValidationService`.

### 1.6 Minimize duplicated state
- Do not keep multiple copies of state unless the duplication is deliberate and justified.
- In Blazor Server especially, duplicated UI/workspace/run state is a bug source.
- `BlazorWorkspace` exists to centralize workstation state ownership; do not create shadow copies of the same state across pages, controllers, and services without an explicit reason.

### 1.7 Explain why
- Comments should explain why the code exists, what invariant it protects, or why a design choice was made.
- Tests should explain what they prove and how.
- Durable reasoning belongs in code comments, commit messages, and GSD decisions, not only in chat.

### 1.8 Naming discipline
- Use precise nouns and verbs.
- Avoid overloaded names where the same term means different things in different contexts.
- Use `snake_case` where the language/style permits it, and preserve clear, consistent naming in C# identifiers and files.
- Add units and qualifiers where they improve correctness.

### 1.9 Back-of-the-envelope thinking first
- Think about bandwidth, latency, memory, CPU, and batching before implementation.
- Do not wait for profiling to discover obvious architectural costs.

---

## 2. Adapt To The Current Stack

These principles are adopted in adapted form because AMS is not a fresh Zig codebase.

### 2.1 Function size discipline
- Prefer functions that fit on one screen.
- In C#, use the Tiger 70-line rule as a strong warning sign, not a rigid law.
- The deeper rule matters more: centralize control flow and keep helpers focused.

### 2.2 No dynamic allocation after initialization
- This is not realistic as a global rule for ASP.NET Core / Blazor / .NET.
- Apply the spirit instead:
  - avoid unnecessary allocation in hot paths
  - bound caches and retained state
  - do not create churny state machines or ad hoc buffers without limits
- For future Zig engine work, this rule can be applied much more literally.

### 2.3 Zero dependencies
- AMS cannot realistically become dependency-free on the current stack.
- But new dependencies must be treated as costs, not conveniences.
- Prefer internal typed abstractions and existing platform capability before adding packages.

### 2.4 Tool minimalism
- Do not multiply tools without a clear reason.
- Prefer the tools already in the repo unless a new one changes the economics materially.

### 2.5 Thin hosts over shared services
- `host/Ams.Cli/Program.cs` and `host/Ams.Workstation.Server/Program.cs` should stay recognizable as composition roots.
- Host adapters such as `CliWorkspace` and `BlazorWorkspace` may translate host-local concerns into shared contracts, but the preferred direction is for orchestration to live in shared Core/application services.
- `PipelineService` and `ValidationService` are the current examples of this direction.
- Where host files remain too large or still carry orchestration debt, record that as audit/refactor input for later slices instead of pretending the migration is already complete.

---

## 3. Future Zig-Specific Rules

These rules should become stricter in the Zig engine and DSP track than in the current .NET application.

- static memory planning where practical
- no surprise allocation in hot paths
- explicit limits everywhere
- tighter function-shape discipline
- stronger compile-time invariant checks
- more literal Tiger-style safety and performance rules

---

## 4. Deferred

These rules matter, but they are **not** binding repo-wide today.
They stay visible here so future work can adopt them deliberately instead of re-debating them.

### 4.1 Full host parity on shared application services
- The CLI composition root already wires shared application services such as `PipelineService` and `ValidationService`.
- The Workstation composition root is moving in the same direction, but it still documents places where service registration parity is incomplete.
- Closing that gap is later engineering work, not something this document should overclaim as finished.

### 4.2 Repo-wide mechanical style gates
- Shared analyzers, warnings-as-errors, automated function-size gates, and contract linting are desirable.
- They are deferred until the current codebase has been audited well enough that enforcing them will improve quality instead of producing noisy repo-wide cleanup.

### 4.3 Large-file and legacy host cleanup
- Oversized host files and incomplete thin-host migrations are real signals.
- They are evidence for later audit/refactor slices, not a reason to silently weaken the contract or to pretend those cleanups happen automatically.

---

## 5. Mechanical Enforcement Requirements

Style is not adopted unless it appears in code and verification.

For any milestone that materially reshapes architecture:
- **Assertions are required**.
- At least **one additional mechanical enforcement surface** is also required:
  - tests where they make sense
  - typed contracts where possible
  - review checklist enforcement at critical seams

Prose alone does not count as adoption.
Current AMS examples include typed seams such as `IWorkspace`, shared orchestration/services such as `PipelineService` and `ValidationService`, and tests that pin durable contract expectations when the repo-level agreement matters.

---

## 6. Review Checklist

When reviewing code, ask:

1. Is the control flow explicit?
2. Are the important limits explicit?
3. Are invariants asserted?
4. Is duplicated state minimized?
5. Are defaults explicit where correctness matters?
6. Do names match the domain cleanly?
7. Do comments explain why, not just what?
8. Does the code reduce or increase M002+ architectural risk?
9. Is the work bounded, or is it trying to clean everything at once?
10. If this code fails, will the operator know what happened?

---

## 7. AMS-Specific Product Rules

- The **Workstation** is the long-term operator surface.
- The **CLI** remains a proving and debugging surface over shared core logic.
- **Prep**, **Proof**, and **Polish** are stage containers, not single features.
- Feature work should compose as modules under those stages rather than redefining stage meaning each time.
- Architecture changes should preserve the path toward benchmarking and a future Zig engine rather than binding AMS more tightly to current accidental structure.

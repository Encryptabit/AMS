# Phase 9: Blazor Audiobook Workstation - Context

**Gathered:** 2026-01-03
**Status:** Ready for research

<vision>
## How This Should Work

A Blazor Server application that becomes the central hub for audiobook production, replacing both the CLI and the standalone validation-viewer. Organized into three areas:

**Prep** - Pipeline orchestration, setting up chapters for processing
**Proof** - Validation and review (port of current validation-viewer)
**Polish** - Take replacement, batch editing, prosody analysis (future)

The playback view is central — time-synced scrolling, keybind shortcuts, and quick export. This is where the narrator's performance gets validated and issues get flagged.

Architecture enables direct access to Ams.Core types and functionality, unlike the standalone validation-viewer. The UI stays decoupled from business logic so adding Blazor WASM later is trivial (just add API endpoints to what Server pages call directly).

The application is designed for extensibility — each area can have modules added over time without compromising the foundational architecture.

</vision>

<essential>
## What Must Be Nailed

- **Clean separation of concerns** — UI decoupled from business logic, ready for WASM migration
- **Three-area architecture** — Prep/Proof/Polish structure that modules plug into
- **Playback view parity** — Time-synced scrolling and keybind interface from validation-viewer must work
- **Ams.Core integration** — Direct access to existing types, pipelines, and functionality

</essential>

<boundaries>
## What's Out of Scope

- Blazor WASM deployment (architecture supports it, but build Server first)
- Full "Prep" area implementation (focus on foundation + Proof first)
- Full "Polish" area implementation (take replacement, prosody — future phases)
- Batch editing across chapters (get single-chapter solid first)
- Mobile/responsive design (desktop workflow)

First phase is **foundation + Proof area** — port validation-viewer capabilities into the new architecture.

</boundaries>

<specifics>
## Specific Ideas

- Port tools/validation-viewer functionality first — audit what's there, categorize into Prep/Proof/Polish
- Keybind + quick export interface is essential UX
- Take replacement workflow: narrator sends corrected takes in response to CRX, seamless to integrate
- Batch editing: highlight same spot across chapters, apply same edit to all
- Prosody compression technology — cool concept to bring to life eventually, but not urgent (often discouraged to mess with narrator timing)
- This app eventually takes over CLI responsibilities too

</specifics>

<notes>
## Additional Context

Current validation-viewer works but is standalone and can't reuse Ams.Core. The new Blazor app solves this by being integrated from the start.

User prioritizes foundation/architecture quality over feature quantity. Better to get the structure right than rush features that won't scale.

The Prep/Proof/Polish mental model maps well to audiobook production workflow and provides natural extension points.

</notes>

---

*Phase: 09-blazor-workstation*
*Context gathered: 2026-01-03*

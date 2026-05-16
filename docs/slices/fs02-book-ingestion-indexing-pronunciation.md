# FS02: Book Ingestion, Indexing, And Pronunciation

Last updated: 2026-05-16

Reader: an engineer changing manuscript parsing, book indexing, cache behavior, or pronunciation enrichment.

Post-read action: record specific FS02 alignment work here before changing parser/indexer contracts, cache rules, pronunciation behavior, or book metadata shape.

## Scope

FS02 owns manuscript parsing, index construction, cached book metadata, pronunciation providers, and proper noun filtering.

## Current Concepts

- Manuscript parsing extracts source text from supported input formats.
- Book indexing builds canonical words, sentences, paragraphs, sections, and totals.
- Book cache stores parse/index results.
- Pronunciation providers enrich book/index data for alignment and MFA.

## Specific Changes Needed

No slice-specific changes recorded yet.

## Decisions

No slice-specific decisions recorded yet.

## Code Sketches

No code sketches recorded yet.

## Open Audit Questions

- Which book-index invariants should move into constructors or factories?
- Which parser/indexer failure shapes are user validation versus internal corruption?
- Which cached book artifacts need compatibility/version handling before refactor?

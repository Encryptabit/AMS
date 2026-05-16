# FS00: Build And Project Surface

Last updated: 2026-05-16

Reader: an engineer changing AMS Core build rules or project-level configuration.

Post-read action: record specific FS00 alignment work here before changing project files, SDK pins, global usings, asset copy rules, or assembly metadata.

## Scope

FS00 owns the Core project boundary: target framework, package references, native/asset copy rules, assembly metadata, global compilation context, and SDK pinning.

## Current Concepts

- Core project file and package surface.
- Assembly metadata.
- Global compilation context.
- SDK and toolchain pins.

## Specific Changes Needed

No slice-specific changes recorded yet.

## Decisions

No slice-specific decisions recorded yet.

## Code Sketches

No code sketches recorded yet.

## Open Audit Questions

- Are build-time asset copy rules explicit enough to make FFmpeg, Tesseract, Silero, and other bundled resources reproducible?
- Are platform-specific build choices documented at the project boundary instead of hidden in runtime code?

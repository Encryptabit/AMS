# Archived Projects

This directory contains dormant projects that are preserved for reference but are not actively maintained or built as part of the main solution.

## Archived Projects

| Project | Original Location | Archive Date | Description |
|---------|------------------|--------------|-------------|
| Ams.UI.Avalonia | host/Ams.UI.Avalonia | 2025-12-30 | Avalonia-based desktop UI (dormant) |
| InspectDocX | out/InspectDocX | 2025-12-30 | DocX inspection utility (dormant) |

## Restoration

To restore a project to active development:

1. Move the project folder back to its original location:
   ```bash
   git mv archive/<ProjectName> <original-location>/<ProjectName>
   ```

2. Add the project to the solution:
   ```bash
   cd host
   dotnet sln add <path-to-csproj>
   ```

3. Restore dependencies and verify build:
   ```bash
   dotnet restore
   dotnet build
   ```

## Notes

- These projects were archived as part of codebase cleanup to reduce build complexity
- Source code is preserved in full for future reference
- Projects are not included in solution builds but remain in version control

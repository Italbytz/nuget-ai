# nuget-ai

`nuget-ai` is the consolidation repository for the refactored `Italbytz.AI.*` package family.

## Current Phase 2 slice

The first migration slice currently includes:

- `Italbytz.AI.Abstractions`
- `Italbytz.AI`

## Planned next slices

- `Italbytz.AI.Agent(.Abstractions)`
- `Italbytz.AI.Search(.Abstractions)`
- `Italbytz.AI.CSP(.Abstractions)`
- `Italbytz.AI.Planning(.Abstractions)`
- `Italbytz.AI.Learning(.Abstractions)`
- `Italbytz.AI.Evolutionary(.Abstractions)`
- `Italbytz.AI.ML*`

## Local validation

```bash
dotnet restore nuget-ai.sln
dotnet test nuget-ai.sln -v minimal
dotnet pack nuget-ai.sln -c Release -v minimal
dotnet tool restore
dotnet tool run docfx docfx/docfx.json
```

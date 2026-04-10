# nuget-ai

`nuget-ai` is the consolidation repository for the refactored `Italbytz.AI.*` package family.

## Current Phase 2 slices

The migration currently includes:

- `Italbytz.AI.Abstractions`
- `Italbytz.AI`
- `Italbytz.AI.Agent.Abstractions`
- `Italbytz.AI.Agent`
- `Italbytz.AI.Search.Abstractions`
- `Italbytz.AI.Search`
- `Italbytz.AI.CSP.Abstractions`
- `Italbytz.AI.CSP`

## Planned next slices

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

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
- `Italbytz.AI.Planning.Abstractions`
- `Italbytz.AI.Planning`
- `Italbytz.AI.Learning.Abstractions`
- `Italbytz.AI.Learning`
- `Italbytz.AI.Evolutionary.Abstractions`
- `Italbytz.AI.Evolutionary`
- `Italbytz.AI.ML.Core`
- `Italbytz.AI.ML`
- `Italbytz.AI.ML.UciDatasets`

## Current follow-up work

- `Explainer`, `Interpreter`, and additional benchmark datasets on top of the now-available `Italbytz.AI.ML.*` trainer and configuration foundation

## Local validation

```bash
dotnet restore nuget-ai.sln
dotnet test nuget-ai.sln -v minimal
dotnet pack nuget-ai.sln -c Release -v minimal
dotnet tool restore
dotnet tool run docfx docfx/docfx.json
```

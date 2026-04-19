# nuget-ai

`nuget-ai` provides reusable AI contracts, algorithms, and ML integrations for .NET applications.

This documentation is intended for package consumers who want to choose the right `Italbytz.AI.*` packages and navigate the available guides and API reference.

## Packages at a glance

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

## Recommended guides

- `Guides > Architecture` explains the package split between abstractions and concrete implementations.
- `Guides > AIMA algorithm index` maps textbook-oriented topics onto the current package structure.
- `Guides > Getting started with ML helpers` shows how to work with `LeastSquaresTrainer`, `Explainer`, and `Interpreter`.

## Use nuget-ai if you want to

- build on shared abstractions for search, CSP, planning, learning, and evolutionary algorithms
- reuse concrete helper packages for agents, solvers, and ML.NET integrations
- load curated UCI-style datasets and inspect trained models
- navigate generated API documentation for the full package family

## Local validation

```bash
dotnet restore nuget-ai.sln
dotnet test nuget-ai.sln -v minimal
dotnet pack nuget-ai.sln -c Release -v minimal
dotnet tool restore
dotnet tool run docfx docfx/docfx.json
```

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

- remaining benchmark helpers and targeted ML-configuration refinements on top of the now-available `Italbytz.AI.ML.*` trainer, explainer, interpreter, configuration foundation, and curated UCI dataset descriptors (`IrisDataset`, `HeartDiseaseDataset`, `WineQualityDataset`, `BreastCancerWisconsinDiagnosticDataset`)

## Guides restored from the legacy repositories

The old `nuget-ports-algorithms-ai`, `nuget-ports-algorithms-aima`, and `nuget-adapters-algorithms-aima` repositories already contained useful orientation material. The most relevant parts are now carried forward in this docfx site:

- `Guides > Architecture` explains the original Ports-and-Adapters / Hexagonal-Architecture rationale behind the split into `*.Abstractions` and concrete packages.
- `Guides > AIMA algorithm index` keeps the textbook-oriented cross-reference from the former AIMA docs and maps it onto the consolidated `Italbytz.AI.*` structure.
- `Guides > Getting started with ML helpers` shows how to use `LeastSquaresTrainer`, `Explainer`, and `Interpreter` without having to reverse-engineer the test suite first.

## Local validation

```bash
dotnet restore nuget-ai.sln
dotnet test nuget-ai.sln -v minimal
dotnet pack nuget-ai.sln -c Release -v minimal
dotnet tool restore
dotnet tool run docfx docfx/docfx.json
```

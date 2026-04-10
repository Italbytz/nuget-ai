# nuget-ai

`nuget-ai` is the target repository for the refactored `Italbytz.AI.*` package family.

## Current migration status

The repository now contains the first ten verified Phase 2 waves:

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

Current follow-up work now focuses on the remaining benchmark helpers and selective ML-configuration refinements; `Explainer`, `Interpreter`, the serializable `Italbytz.AI.ML.Core.Configuration` layer, and curated `Iris`, `Heart Disease`, `Wine Quality`, and `Breast Cancer Wisconsin Diagnostic` dataset descriptors are already available.

## Which package should I use?

- Use `Italbytz.AI.Abstractions` for general solver and metrics contracts.
- Use `Italbytz.AI` for shared runtime helpers such as metrics and thread-safe randomness.
- Use `Italbytz.AI.Agent.Abstractions` when you only need agent-facing contracts.
- Use `Italbytz.AI.Agent` for basic agent and environment helper implementations.
- Use `Italbytz.AI.Search.Abstractions` for search/problem contracts.
- Use `Italbytz.AI.Search` for concrete search helpers such as `SearchAgent`, `GeneralProblem`, breadth-first search, and uniform-cost search.
- Use `Italbytz.AI.CSP.Abstractions` for constraint-satisfaction contracts such as `ICSP`, `IAssignment`, `IDomain`, and `IConstraint`.
- Use `Italbytz.AI.CSP` for concrete CSP building blocks and solvers such as `CSP`, `MapCSP`, `FlexibleBacktrackingSolver`, `TreeCspSolver`, and `MinConflictsSolver`.
- Use `Italbytz.AI.Planning.Abstractions` for planning contracts such as `IActionSchema`, `IState`, `IPlanningProblem`, and the minimal FOL term/literal abstractions they rely on.
- Use `Italbytz.AI.Planning` for planning parsers and implementations such as `ActionSchema`, `State`, `PlanningProblem`, `HierarchicalSearchAlgorithm`, and `PlanningProblemFactory`.
- Use `Italbytz.AI.Learning.Abstractions` for dataset, attribute, and learner contracts such as `IDataSet`, `IExample`, `ILearner`, `IParameterizedLearner`, and `ICrossValidation`.
- Use `Italbytz.AI.Learning` for concrete learning helpers and algorithms such as `DataSetFactory`, `DataSetSpecification`, `MajorityLearner`, `DecisionTreeLearner`, `DecisionTree`, and `CrossValidation`.
- Use `Italbytz.AI.Evolutionary.Abstractions` for evolutionary-algorithm contracts such as `IFitnessFunction`, `IFitnessValue`, `IIndividual`, `IGenotype`, `ISearchSpace`, `ILiteral`, `IMonomial`, and `IPolynomial`.
- Use `Italbytz.AI.Evolutionary` for core evolutionary implementations such as `SingleFitnessValue`, `OneMax`, `BitString`, `BitStringGenotype`, `SetLiteral`, `WeightedMonomial`, `WeightedPolynomial`, and `WeightedPolynomialGenotype`.
- Use `Italbytz.AI.ML.Core` for shared ML.NET helpers and configuration DTOs such as `ThreadSafeMLContext`, `DataExcerpt`, `CategoricalFeature`, `NumericalFeature`, `TrainingConfiguration`, `TabularFileDataSourceV3`, metrics, default column names, and lookup-map utilities.
- Use `Italbytz.AI.ML` for Learning-backed ML.NET integrations, trainers, and model-inspection helpers such as `DecisionTreeBinaryTrainer`, `DecisionTreeMulticlassTrainer`, `LeastSquaresTrainer`, `Explainer`, and `Interpreter`.
- Use `Italbytz.AI.ML.UciDatasets` for curated UCI-style dataset loaders and preprocessing helpers such as `IrisDataset`, `HeartDiseaseDataset`, `WineQualityDataset`, `BreastCancerWisconsinDiagnosticDataset`, and the shared `Data` registry.

## Migration notice

Older repositories and articles may still refer to names such as:

- `Italbytz.Ports.Algorithms`
- `Italbytz.Adapters.Algorithms`
- `Italbytz.Ports.Algorithms.AI`
- `Italbytz.Adapters.Algorithms.AI`
- `Italbytz.Ports.Algorithms.AIMA`
- `Italbytz.Adapters.Algorithms.AIMA`
- `Italbytz.Ports.Algorithms.EA`
- `Italbytz.Adapters.Algorithms.EA`
- `Italbytz.ML`
- `Italbytz.ML.UCIMLR`
- `Italbytz.Adapters.Algorithms.ML`

For all new development, please use the new `Italbytz.AI.*` package family.

## Starter scenarios and examples

The older `csharp-mstest-ai` starter repository is being folded into this repo step by step.
A good starting point for consumer-style search examples is now:

- `tests/Italbytz.AI.Tests/RomaniaMapSearchIntegrationTests.cs`

This keeps the examples close to the actual `Italbytz.AI.*` packages instead of maintaining a second legacy repo line with outdated package names.

## Documentation

API documentation is generated with `docfx` and can be published via GitHub Pages:

- `https://italbytz.github.io/nuget-ai/`

The doc site now also carries forward the most useful orientation material from the older AI/AIMA repositories:

- an architecture guide for the historic Ports-and-Adapters split
- an AIMA-oriented algorithm index
- an ML quickstart for `LeastSquaresTrainer`, `Explainer`, and `Interpreter`

If the URL still returns `404`, wait until the `CI` workflow on `main` has completed the first Pages publish run.

## Quality checks

This repository includes:

- a `GitHub Actions` workflow in `.github/workflows/ci.yml`
- automated `restore`, `build`, `test`, `pack`, and docs generation
- a `docfx` setup under `docfx/`

## Release model

- the current `nuget-ai` line stays on `1.0.0-preview.*` while the broader AI migration is still in progress
- a pushed tag such as `v1.0.0-preview.2` triggers the release-ready pipeline in GitHub Actions
- if the repository secret `NUGET_API_KEY` is configured, the workflow also publishes `.nupkg` and `.snupkg` files to NuGet

## Local validation

```bash
dotnet restore nuget-ai.sln
dotnet test nuget-ai.sln -v minimal
dotnet pack nuget-ai.sln -c Release -v minimal
dotnet tool restore
dotnet tool run docfx docfx/docfx.json
```
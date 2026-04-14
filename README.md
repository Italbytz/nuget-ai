# nuget-ai

[![Documentation](https://img.shields.io/badge/Documentation-GitHub%20Pages-blue?style=for-the-badge)](https://italbytz.github.io/nuget-ai/)

`nuget-ai` is the target repository for the refactored `Italbytz.AI.*` package family.

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
- Use `Italbytz.AI.ML.LogicGp` for the supported LogicGP trainer facade such as `LogicGpGpasBinaryTrainer`, `LogicGpFlcwMacroMulticlassTrainer`, `LogicGpFlcwMicroMulticlassTrainer`, `LogicGpRlcwMacroMulticlassTrainer`, and `LogicGpRlcwMicroMulticlassTrainer`.

The `Italbytz.AI.ML.LogicGp.Internal.*` namespaces are implementation details of that facade and are not intended as a stable consumer API.
For shared loading and serialization entry points, use `LogicGpTrainer<TOutput>` and `LogicGp.LoadTrainer<TOutput>(...)` from the root `Italbytz.AI.ML.LogicGp` namespace.

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

## Legacy consumer migrations

- the former `csharp-mstest-ai` starter repository is no longer maintained as a standalone consumer; its migrated legacy sample snapshot now lives under `samples/legacy-aima-mstest/`, while the active replacements live in `tests/Italbytz.AI.Tests/`
- the former `csharp-mstest-logicgp` starter repository is no longer maintained as a standalone consumer; its migrated legacy benchmark snapshot now lives under `benchmarks/legacy-logicgp-regression/`, while compact replacement coverage lives in `tests/Italbytz.AI.Tests/MlIntegrationTests.cs`
- a short migration map for both removals is documented in `tests/LEGACY_CONSUMER_MIGRATION.md`

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
